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
        private int _attemptsCount = 0;
        private string? _currentTargetImageUrl;

        public PictureMode(GameService gameService)
        {
            InitializeComponent();

            _gameService = gameService;
            _guesses = new ObservableCollection<GuessResult>();
            GuessesList.ItemsSource = _guesses;

            InitZoomOffsets();

            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                Command = new Command(async () => await ShowExitConfirmAsync())
            });
        }

        private void InitZoomOffsets()
        {
            Random random = new Random();
            randomZoomX = random.Next(-100, 100);
            randomZoomY = random.Next(-100, 100);
        }

        protected override bool OnBackButtonPressed()
        {
            _ = ShowExitConfirmAsync();
            return true;
        }

        private async Task ShowExitConfirmAsync()
        {
            ExitConfirmOverlay.IsVisible = true;
            await ExitConfirmOverlay.FadeTo(1, 200, Easing.CubicOut);
        }

        private async void OnExitCancelTapped(object sender, TappedEventArgs e)
        {
            await ExitConfirmOverlay.FadeTo(0, 150, Easing.CubicIn);
            ExitConfirmOverlay.IsVisible = false;
        }

        private async void OnExitConfirmTapped(object sender, TappedEventArgs e)
        {
            await ExitConfirmOverlay.FadeTo(0, 150, Easing.CubicIn);
            ExitConfirmOverlay.IsVisible = false;
            await Shell.Current.GoToAsync("..");
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
            _attemptsCount++;

            SearchEntry.Text = string.Empty;
            SuggestionsList.IsVisible = false;

            // Najnowsza próba trafia na początek listy - przewijamy tam widok,
            // żeby nie trzeba było scrollować ręcznie.
            GuessesList.ScrollTo(0, position: ScrollToPosition.Start, animate: true);

            if (result.NameColor == "Green")
            {
                await ShowWinOverlayAsync(selectedCeleb.Name ?? "");
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
                    _currentTargetImageUrl = target.ImageUrl;
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
                "Problem z połączeniem",
                "Nie udało się wczytać żadnego zdjęcia. Sprawdź połączenie z internetem i spróbuj ponownie.",
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

        private async Task ShowWinOverlayAsync(string celebrityName)
        {
            WinNameLabel.Text = celebrityName;
            WinAttemptsLabel.Text = _attemptsCount == 1
                ? "Zgadłeś za pierwszym razem!"
                : $"Zgadłeś w {_attemptsCount} próbach.";

            // Pokazujemy pełne, nieprzybliżone zdjęcie w karcie z gratulacjami
            WinImage.Source = _currentTargetImageUrl;

            WinOverlay.IsVisible = true;
            WinCard.Scale = 0.9;

            await Task.WhenAll(
                WinOverlay.FadeTo(1, 200, Easing.CubicOut),
                WinCard.ScaleTo(1, 250, Easing.SpringOut)
            );
        }

        private async void OnPlayAgainTapped(object sender, TappedEventArgs e)
        {
            await HideWinOverlayAsync();

            _guesses.Clear();
            _attemptsCount = 0;

            InitZoomOffsets();
            _gameService.StartNewGame();
            await LoadTargetImageWithTimeoutAsync();
        }

        private async void OnBackToMenuTapped(object sender, TappedEventArgs e)
        {
            await HideWinOverlayAsync();
            await Shell.Current.GoToAsync("..");
        }

        private async Task HideWinOverlayAsync()
        {
            await Task.WhenAll(
                WinOverlay.FadeTo(0, 150, Easing.CubicIn),
                WinCard.ScaleTo(0.9, 150, Easing.CubicIn)
            );
            WinOverlay.IsVisible = false;
        }
    }
}