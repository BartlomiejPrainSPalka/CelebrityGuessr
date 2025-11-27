using CelebrityGuessr.Services;
using CelebrityGuessr.Modes;

namespace CelebrityGuessr
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<GameService>();

            builder.Services.AddTransient<NormalMode>();
            builder.Services.AddTransient<PictureMode>();

            return builder.Build();
        }
    }
}