using CelebrityGuessr.Models;
using CelebrityGuessr.Services;
using System.Collections.ObjectModel;

namespace CelebrityGuessr.Modes
{
    public partial class NormalMode : ContentPage
    {
        private readonly GameService _gameService;
        private ObservableCollection<GuessResult> _guesses;
        private const double SingleSuggestionHeight = 45;
        private const int MaxSuggestionsToShow = 4;

        private const string HideHelpPrefKey = "NormalMode_HideHelpOnStart";
        private int _attemptsCount = 0;

        public NormalMode(GameService gameService)
        {
            InitializeComponent();

            _gameService = gameService;
            _guesses = new ObservableCollection<GuessResult>();
            GuessesList.ItemsSource = _guesses;

            Shell.SetBackButtonBehavior(this, new BackButtonBehavior
            {
                Command = new Command(async () => await ShowExitConfirmAsync())
            });
        }

        protected override async void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            await _gameService.InitializeDatabase();
            _gameService.StartNewGame();
            _attemptsCount = 0;

            bool hideHelp = Preferences.Get(HideHelpPrefKey, false);
            if (!hideHelp)
            {
                await ShowHelpOverlayAsync();
            }
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

        // Pokazuje overlay z podpowiedzią (użyte zarówno automatycznie, jak i po kliknięciu "?")
        private async void OnHelpTapped(object sender, TappedEventArgs e)
        {
            await ShowHelpOverlayAsync();
        }

        private async Task ShowHelpOverlayAsync()
        {
            DontShowAgainSwitch.IsToggled = Preferences.Get(HideHelpPrefKey, false);

            HelpOverlay.IsVisible = true;
            await HelpOverlay.FadeTo(1, 200, Easing.CubicOut);
        }

        // Kliknięcie w przyciemnione tło zamyka overlay
        private async void OnHelpOverlayBackgroundTapped(object sender, TappedEventArgs e)
        {
            await CloseHelpOverlayAsync();
        }

        // Kliknięcie w samą kartę nie powinno zamykać overlaya (tylko tło)
        private void OnHelpCardTapped(object sender, TappedEventArgs e)
        {
            // celowo puste - zatrzymuje "bąbelkowanie" zdarzenia do tła
        }

        private async void OnHelpCloseTapped(object sender, TappedEventArgs e)
        {
            await CloseHelpOverlayAsync();
        }

        private async Task CloseHelpOverlayAsync()
        {
            Preferences.Set(HideHelpPrefKey, DontShowAgainSwitch.IsToggled);

            await HelpOverlay.FadeTo(0, 150, Easing.CubicIn);
            HelpOverlay.IsVisible = false;
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

            // Najnowsza karta trafia zawsze na sam początek listy (index 0),
            // więc przewijamy tam widok, żeby gracz nie musiał scrollować ręcznie.
            GuessesList.ScrollTo(0, position: ScrollToPosition.Start, animate: true);

            if (result.NameColor == "Green")
            {
                await ShowWinOverlayAsync(selectedCeleb.Name ?? "");
            }
        }

        private async Task ShowWinOverlayAsync(string celebrityName)
        {
            WinNameLabel.Text = celebrityName;
            WinAttemptsLabel.Text = _attemptsCount == 1
                ? "Zgadłeś za pierwszym razem!"
                : $"Zgadłeś w {_attemptsCount} próbach.";

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
            _gameService.StartNewGame();
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