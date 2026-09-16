namespace CelebrityGuessr.About;

public partial class Creators : ContentPage
{
    public Creators()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CreatorsCard.FadeTo(1, 350, Easing.CubicOut);
        _ = CreatorsCard.ScaleTo(1, 350, Easing.SpringOut);
    }
}