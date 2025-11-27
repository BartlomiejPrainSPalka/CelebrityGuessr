using CelebrityGuessr.Modes;
namespace CelebrityGuessr
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent(); 
            Routing.RegisterRoute(nameof(PictureMode), typeof(PictureMode));
            Routing.RegisterRoute(nameof(NormalMode), typeof(NormalMode));
        }
    }
}
