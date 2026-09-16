using CelebrityGuessr.Models;
using CelebrityGuessr.Services;
using System.Collections.ObjectModel;

namespace CelebrityGuessr.Modes
{
    public partial class PictureMode : ContentPage
    {
        private readonly GameService _gameService;
        private ObservableCollection<GuessResult> _guesses;
        private const double SingleSuggestionHeight = 45;
        private const int MaxSuggestionsToShow = 4;

        // Czas, po którym uznajemy, że zdjęcie się nie wczytało, i losujemy nową osobę
        private const int ImageLoadTimeoutSeconds = 5;
        // Zabezpieczenie przed nieskończoną pętlą, gdyby wszystkie zdjęcia były niedostępne
        // (np. brak internetu) - po tylu próbach pokazujemy komunikat zamiast losować w kółko
        private const int MaxLoadAttempts = 8;

        private static readonly HttpClient _httpClient = CreateHttpClient();

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            // Wikimedia Commons (i część innych serwerów) odrzuca żądania bez nagłówka
            // User-Agent zwracając 403 Forbidden. Bez tego część zdjęć nigdy się nie wczyta.
            client.DefaultRequestHeaders.UserAgent.ParseAdd("CelebrityGuessrApp/1.0 (contact: example@example.com)");
            return client;
        }

        int randomZoomX;
        int randomZoomY;

        public PictureMode(GameService gameService)
        {
            InitializeComponent();

            _gameService = gameService;
            _guesses = new ObservableCollection<GuessResult>();
            GuessesList.ItemsSource = _guesses;

            Random random = new Random();
            randomZoomX = random.Next(-100, 100);
            randomZoomY = random.Next(-100, 100);

            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                Command = new Command(async () => await ConfirmExit())
            });
        }

        protected override bool OnBackButtonPressed()
        {
            Dispatcher.Dispatch(async () => await ConfirmExit());
            return true;
        }

        private async Task ConfirmExit()
        {
            bool answer = await DisplayAlert(
                "Wyjœcie",
                "Czy na pewno chcesz wyjœæ? Stracisz progres oraz wylosowana zostanie nowa osoba.",
                "Tak, wyjdŸ",
                "Anuluj");

            if (answer)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            string text = e.NewTextValue;

            if (string.IsNullOrWhiteSpace(text))
            {
                SuggestionsList.IsVisible = false;
                SuggestionsList.HeightRequest = 0;
                SuggestionsList.ItemsSource = null;
                return;
            }

            var matches = _gameService.SearchCelebrities(text);

            if (matches.Any())
            {
                SuggestionsList.ItemsSource = matches;

                int displayCount = Math.Min(matches.Count, MaxSuggestionsToShow);
                double desiredHeight = displayCount * SingleSuggestionHeight;

                SuggestionsList.IsVisible = true;
                SuggestionsList.HeightRequest = desiredHeight;
            }
            else
            {
                SuggestionsList.IsVisible = false;
                SuggestionsList.HeightRequest = 0;
            }
        }

        private async void OnSuggestionTapped(object sender, TappedEventArgs e)
        {
            var selectedCeleb = e.Parameter as Celebrity;

            if (selectedCeleb == null)
                return;

            var result = _gameService.CheckGuess(selectedCeleb);

            _guesses.Insert(0, result);

            SearchEntry.Text = string.Empty;
            SuggestionsList.IsVisible = false;

            if (result.NameColor == "Green")
            {
                ShowFullImage(selectedCeleb.ImageUrl ?? "not avaible");

                await DisplayAlert("Gratulacje!", $"Zgad³eœ! To {selectedCeleb.Name}", "OK");

                await Task.Delay(500);
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                if (ClueImage.Scale != 1.5)
                {
                    ClueImage.Scale -= 0.5;

                    randomZoomX /= 2;
                    ClueImage.TranslationX = randomZoomX;

                    randomZoomY /= 2;
                    ClueImage.TranslationY = randomZoomY;
                }
            }
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            await _gameService.InitializeDatabase();

            await LoadTargetImageWithTimeoutAsync();
        }

        // Pobiera zdjęcie wylosowanej osoby. Jeśli nie uda się go wczytać w ciągu
        // ImageLoadTimeoutSeconds (np. martwy link, brak internetu), losuje nową osobę
        // i próbuje ponownie - aż do skutku lub osiągnięcia limitu prób.
        private async Task LoadTargetImageWithTimeoutAsync()
        {
            for (int attempt = 0; attempt < MaxLoadAttempts; attempt++)
            {
                var target = _gameService.GetTargetCelebrity();

                if (target == null || string.IsNullOrEmpty(target.ImageUrl))
                {
                    _gameService.StartNewGame();
                    continue;
                }

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(ImageLoadTimeoutSeconds));

                try
                {
                    byte[] imageBytes = await _httpClient.GetByteArrayAsync(target.ImageUrl, cts.Token);

                    ClueImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                    ApplyRandomZoom();
                    return;
                }
                catch (Exception)
                {
                    // Timeout (5s) albo błąd pobierania - losujemy inną osobę i próbujemy dalej
                    _gameService.StartNewGame();
                }
            }

            // Wszystkie próby zawiodły - prawdopodobnie brak internetu
            await DisplayAlert(
                "Nie udało wczytać się obrazu",
                "Sprawdź połączenie z internetem lub spróbuj ponownie.",
                "OK");

            await Shell.Current.GoToAsync("..");
        }

        private void ApplyRandomZoom()
        {
            ClueImage.Scale = 1;
            ClueImage.TranslationX = 0;
            ClueImage.TranslationY = 0;

            ClueImage.Scale = 3;

            ClueImage.TranslationX = randomZoomX;
            ClueImage.TranslationY = randomZoomY;
        }

        private void ShowFullImage(string imageUrl)
        {
            ClueImage.Source = imageUrl;
            ClueImage.Scale = 1;
            ClueImage.TranslationX = 0;
            ClueImage.TranslationY = 0;
        }
    }
}