using CelebrityGuessr.Modes;
using CelebrityGuessr.About;
namespace CelebrityGuessr
{
    public partial class MainPage : ContentPage
    {
        private bool _isExpanded1 = false;
        private bool _isExpanded2 = false;
        private const uint AnimationDuration = 300;
        private const double ExpandedHeight = 101;

        public MainPage()
        {
            InitializeComponent();
            SubButtonsContainer.HeightRequest = 0;
            SubButtonsContainer.IsVisible = true;
            MainButtonIcon.Rotation = 0;

            //ustawienia 2 menu przycisków
            SubButtonsContainer2.HeightRequest = 0;
            SubButtonsContainer2.IsVisible = true;
            MainButtonIcon2.Rotation = 0;
        }

        private void OnMainButtonClicked(object sender, EventArgs e)
        {
            _isExpanded1 = !_isExpanded1;

            if (_isExpanded1)
            {
                MainButtonIcon.RotateTo(90, AnimationDuration, Easing.SinOut);


                var expandAnimation = new Animation(v => SubButtonsContainer.HeightRequest = v, 0, ExpandedHeight, Easing.CubicOut);

                expandAnimation.Commit(this, "ExpandMenu", length: AnimationDuration);
            }
            else
            {
                MainButtonIcon.RotateTo(0, AnimationDuration, Easing.SinIn);

                var collapseAnimation = new Animation(v => SubButtonsContainer.HeightRequest = v, ExpandedHeight, 0, Easing.CubicIn);

                collapseAnimation.Commit(this, "CollapseMenu", length: AnimationDuration, finished: (v, b) =>
                {
                    
                });
            }
        }

        private void OnMainButtonClicked2(object sender, EventArgs e)
        {
            _isExpanded2 = !_isExpanded2;

            if (_isExpanded2)
            {
                MainButtonIcon2.RotateTo(90, AnimationDuration, Easing.SinOut);

                var expandAnimation = new Animation(v => SubButtonsContainer2.HeightRequest = v, 0, ExpandedHeight, Easing.CubicOut);

                expandAnimation.Commit(this, "ExpandMenu", length: AnimationDuration);
            }
            else
            {
                MainButtonIcon2.RotateTo(0, AnimationDuration, Easing.SinIn);

                var collapseAnimation = new Animation(v => SubButtonsContainer2.HeightRequest = v, ExpandedHeight, 0, Easing.CubicIn);

                collapseAnimation.Commit(this, "CollapseMenu", length: AnimationDuration, finished: (v, b) =>
                {
                });
            }
        }

        private async void OnNormalClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                button.BackgroundColor = Color.FromArgb("#FFC107");
                await button.ScaleTo(1.05, 50);
                await button.ScaleTo(1.0, 50);
                button.BackgroundColor = Color.FromArgb("#1c1c1c");
                await Shell.Current.GoToAsync(nameof(NormalMode));
            }
        }


        private async void OnPictureClicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                button.BackgroundColor = Color.FromArgb("#FFC107");
                await button.ScaleTo(1.05, 50);
                await button.ScaleTo(1.0, 50);
                button.BackgroundColor = Color.FromArgb("#1c1c1c");
                await Shell.Current.GoToAsync(nameof(PictureMode));
            }
        }

        private async void OnAboutGameClicked(object sender, EventArgs e)
        {
            var aboutGame = new AboutGame();
            if (sender is Button button)
            {
                button.BackgroundColor = Color.FromArgb("#FFC107");
                await button.ScaleTo(1.05, 50);
                await button.ScaleTo(1.0, 50);
                button.BackgroundColor = Color.FromArgb("#1c1c1c");
                await Navigation.PushAsync(aboutGame);
            }
        }

        private async void OnCreatorsClicked(object sender, EventArgs e)
        {
            var creators = new Creators();
            if (sender is Button button)
            {
                button.BackgroundColor = Color.FromArgb("#FFC107");
                await button.ScaleTo(1.05, 50);
                await button.ScaleTo(1.0, 50);
                button.BackgroundColor = Color.FromArgb("#1c1c1c");
                await Navigation.PushAsync(creators);
            }
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {

        }
    }
}
