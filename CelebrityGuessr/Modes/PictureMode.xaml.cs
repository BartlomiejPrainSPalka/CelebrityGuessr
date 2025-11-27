using CelebrityGuessr.Models;
using CelebrityGuessr.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CelebrityGuessr.Modes
{
    public partial class PictureMode : ContentPage
    {
        private readonly GameService _gameService;
        private ObservableCollection<GuessResult> _guesses;

        public PictureMode(GameService gameService)
        {
            InitializeComponent();

            ClueImage.Source = _gameService.GetTargetCelebrityImageUrl();

            _gameService = gameService;
            _guesses = new ObservableCollection<GuessResult>();
            GuessesList.ItemsSource = _guesses;
            

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
                SuggestionsList.HeightRequest = 0;
                return;
            }

            var matches = _gameService.SearchCelebrities(text);

            if (matches.Any())
            {
                SuggestionsList.ItemsSource = matches;
                SuggestionsList.HeightRequest = 100;
            }
            else
            {
                SuggestionsList.HeightRequest = 0;
                SuggestionsList.ItemsSource = null;
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
                ShowFullImage(selectedCeleb.ImageUrl);

                await DisplayAlert("Gratulacje!", $"Zgad³eœ! To {selectedCeleb.Name}", "OK");

                await Task.Delay(1500);
                await Shell.Current.GoToAsync("..");
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

            var random = new Random();
            ClueImage.TranslationX = random.Next(-100, 100);
            ClueImage.TranslationY = random.Next(-100, 100);
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