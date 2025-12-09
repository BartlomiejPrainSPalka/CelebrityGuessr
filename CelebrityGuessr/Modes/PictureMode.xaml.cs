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
                if(ClueImage.Scale != 1.5)
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

            var target = _gameService.GetTargetCelebrity();

            if (target != null && !string.IsNullOrEmpty(target.ImageUrl))
            {
                ClueImage.Source = target.ImageUrl;
                ApplyRandomZoom();
            }
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