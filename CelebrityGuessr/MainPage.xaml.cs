using CelebrityGuessr.Modes;
using CelebrityGuessr.About;

namespace CelebrityGuessr
{
    public partial class MainPage : ContentPage
    {
        private bool _isExpanded1 = false;
        private bool _isExpanded2 = false;
        private const uint AnimationDuration = 300;
        private const double ExpandedHeight = 135;

        public MainPage()
        {
            InitializeComponent();

            SubButtonsContainer.HeightRequest = 0;
            SubButtonsContainer.IsVisible = true;
            MainButtonIcon.Rotation = 0;

            SubButtonsContainer2.HeightRequest = 0;
            SubButtonsContainer2.IsVisible = true;
            MainButtonIcon2.Rotation = 0;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ResetMenuState();
            _ = PlayEntranceAnimationAsync();
        }

        // Wraca do menu w stanie "czystym" - zwinięte podmenu, ikony w pozycji wyjściowej.
        // Ważne przy powrocie z trybu gry (np. przyciskiem "Wróć do menu"), żeby gracz
        // nie trafiał na przypadkiem rozwinięte podmenu sprzed wejścia do gry.
        private void ResetMenuState()
        {
            _isExpanded1 = false;
            SubButtonsContainer.HeightRequest = 0;
            MainButtonIcon.Rotation = 0;

            _isExpanded2 = false;
            SubButtonsContainer2.HeightRequest = 0;
            MainButtonIcon2.Rotation = 0;
        }

        // Płynne, stopniowane wejście: logo -> karta 1 -> karta 2
        private async Task PlayEntranceAnimationAsync()
        {
            LogoImage.Opacity = 0;
            LogoImage.Scale = 0.9;
            Card1.Opacity = 0;
            Card1.TranslationY = 24;
            Card2.Opacity = 0;
            Card2.TranslationY = 24;

            await Task.WhenAll(
                LogoImage.FadeTo(1, 400, Easing.CubicOut),
                LogoImage.ScaleTo(1, 400, Easing.CubicOut)
            );

            await Task.WhenAll(
                Card1.FadeTo(1, 350, Easing.CubicOut),
                Card1.TranslateTo(0, 0, 350, Easing.CubicOut)
            );

            await Task.WhenAll(
                Card2.FadeTo(1, 350, Easing.CubicOut),
                Card2.TranslateTo(0, 0, 350, Easing.CubicOut)
            );
        }

        private void OnMainButtonClicked(object sender, EventArgs e)
        {
            _isExpanded1 = !_isExpanded1;
            ToggleSubMenu(SubButtonsContainer, MainButtonIcon, _isExpanded1);
        }

        private void OnMainButtonClicked2(object sender, EventArgs e)
        {
            _isExpanded2 = !_isExpanded2;
            ToggleSubMenu(SubButtonsContainer2, MainButtonIcon2, _isExpanded2);
        }

        // Wspólna logika rozwijania/zwijania podmenu, żeby nie duplikować kodu
        private void ToggleSubMenu(VerticalStackLayout container, Image icon, bool expand)
        {
            if (expand)
            {
                icon.RotateTo(90, AnimationDuration, Easing.SinOut);

                var expandAnimation = new Animation(v => container.HeightRequest = v, 0, ExpandedHeight, Easing.CubicOut);
                expandAnimation.Commit(this, "ExpandMenu" + container.Id, length: AnimationDuration);
            }
            else
            {
                icon.RotateTo(0, AnimationDuration, Easing.SinIn);

                var collapseAnimation = new Animation(v => container.HeightRequest = v, ExpandedHeight, 0, Easing.CubicIn);
                collapseAnimation.Commit(this, "CollapseMenu" + container.Id, length: AnimationDuration);
            }
        }

        private async void OnNormalClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                await AnimateButtonPress(button);
                await Shell.Current.GoToAsync(nameof(NormalMode));
            }
        }

        private async void OnPictureClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                await AnimateButtonPress(button);
                await Shell.Current.GoToAsync(nameof(PictureMode));
            }
        }

        private async void OnAboutGameClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                await AnimateButtonPress(button);
                await Navigation.PushAsync(new AboutGame());
            }
        }

        private async void OnCreatorsClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                await AnimateButtonPress(button);
                await Navigation.PushAsync(new Creators());
            }
        }

        // Krótkie, "sprężyste" podświetlenie i skala przycisku po kliknięciu
        private static async Task AnimateButtonPress(Button button)
        {
            var originalColor = button.BackgroundColor;

            button.BackgroundColor = Color.FromArgb("#FFC107");
            await Task.WhenAll(
                button.ScaleTo(1.05, 60, Easing.CubicOut),
                button.FadeTo(0.85, 60)
            );
            await Task.WhenAll(
                button.ScaleTo(1.0, 90, Easing.CubicIn),
                button.FadeTo(1.0, 90)
            );

            button.BackgroundColor = originalColor;
        }
    }
}