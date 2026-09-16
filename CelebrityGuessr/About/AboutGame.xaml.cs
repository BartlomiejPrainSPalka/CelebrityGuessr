namespace CelebrityGuessr.About;

public partial class AboutGame : ContentPage
{
    public AboutGame()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = ContentRoot.FadeTo(1, 350, Easing.CubicOut);
        _ = ContentRoot.TranslateTo(0, 0, 350, Easing.CubicOut);
    }
}