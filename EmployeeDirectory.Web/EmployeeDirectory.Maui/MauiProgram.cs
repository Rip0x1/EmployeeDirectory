using EmployeeDirectory.Maui.Services;
using EmployeeDirectory.Maui.ViewModels;
using Microsoft.Extensions.Logging;

namespace EmployeeDirectory.Maui;

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

        builder.Services.AddSingleton<HttpsClientHandlerService>();

        builder.Services.AddSingleton(sp =>
        {
            var handlerService = sp.GetRequiredService<HttpsClientHandlerService>();

            string baseAddress = DeviceInfo.Platform == DevicePlatform.Android
                                 ? "https://10.0.2.2:7019/"
                                 : "https://localhost:7019/";

            return new HttpClient(handlerService.GetPlatformMessageHandler())
            {
                BaseAddress = new Uri(baseAddress)
            };
        });

        builder.Services.AddSingleton<EmployeeApiService>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        return builder.Build();
    }
}