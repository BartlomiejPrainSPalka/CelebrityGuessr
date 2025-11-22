using CelebrityGuessr.Models;
using CelebrityGuessr.Services;
using System.Collections.ObjectModel;

namespace CelebrityGuessr
{
    public partial class NormalMode : ContentPage
    {
        private readonly GameService _gameService;
        private ObservableCollection<GuessResult> _guesses;

        public NormalMode()
        {
            InitializeComponent();

            _gameService = new GameService();
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
                SuggestionsList.IsVisible = false;
                return;
            }

            var matches = _gameService.SearchCelebrities(text);

            if (matches.Any())
            {
                SuggestionsList.ItemsSource = matches;
                SuggestionsList.IsVisible = true;
            }
            else
            {
                SuggestionsList.IsVisible = false;
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
                await DisplayAlert("Gratulacje!", $"Zgad³eœ! To {selectedCeleb.Name}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}