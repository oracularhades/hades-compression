using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

namespace HadesCompression
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
//                 .ConfigureLifecycleEvents(events =>
//                 {
// #if WINDOWS
//                     events.AddWindows(windows => windows
//                     .OnWindowCreated(window =>
//                     {
//                         window.Destroying += MainPage.window_closed;
//                     }));
// #endif
//                 })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
