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

            // 1. ZABEZPIECZENIE PRZYCISKU WSTECZ (Strza³ka w lewym górnym rogu)
            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                Command = new Command(async () => await ConfirmExit())
            });
        }

        // 2. ZABEZPIECZENIE FIZYCZNEGO PRZYCISKU WSTECZ (Android)
        protected override bool OnBackButtonPressed()
        {
            // Uruchamiamy asynchroniczne pytanie w g³ównym w¹tku UI
            Dispatcher.Dispatch(async () => await ConfirmExit());

            // Zwracamy 'true', co oznacza: "systemie, nie zamykaj strony, ja to obs³u¿ê rêcznie"
            return true;
        }

        // Wspólna metoda pytaj¹ca o wyjœcie
        private async Task ConfirmExit()
        {
            bool answer = await DisplayAlert(
                "Wyjœcie",
                "Czy na pewno chcesz wyjœæ? Stracisz progres oraz wylosowana zostanie nowa osoba.",
                "Tak, wyjdŸ",
                "Anuluj");

            if (answer)
            {
                // Jeœli u¿ytkownik potwierdzi, cofamy siê do menu g³ównego
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

        // Zmieniliœmy void na 'async void', aby móc u¿yæ await w œrodku
        private async void OnSuggestionTapped(object sender, TappedEventArgs e)
        {
            // Pobieramy celebrytê z parametru przes³anego z XAML
            var selectedCeleb = e.Parameter as Celebrity;

            if (selectedCeleb == null)
                return;

            // 1. Logika gry
            var result = _gameService.CheckGuess(selectedCeleb);

            // 2. Dodajemy wynik na górê
            _guesses.Insert(0, result);

            // 3. Czyœcimy UI
            SearchEntry.Text = string.Empty;
            SuggestionsList.IsVisible = false;

            // 4. Obs³uga wygranej
            if (result.NameColor == "Green")
            {
                await DisplayAlert("Gratulacje!", $"Zgad³eœ! To {selectedCeleb.Name}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
    }
}