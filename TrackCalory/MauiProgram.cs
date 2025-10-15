using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace TrackCalory;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            // Initialize the .NET MAUI Community Toolkit by adding the below line of code
            .UseMauiCommunityToolkit()
            // After initializing the .NET MAUI Community Toolkit, optionally add additional fonts
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Continue initializing your .NET MAUI App here

        //return builder.Build();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        // ========== РЕЄСТРАЦІЯ СЕРВІСІВ ==========

        // Визначаємо шлях до БД
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TrackCalory.db3");

        // Реєструємо DatabaseService з конкретним шляхом до БД
        builder.Services.AddSingleton(s => new Services.DatabaseService(dbPath));

        // Реєструємо CalorieDataService з залежністю від DatabaseService
        builder.Services.AddSingleton<Services.CalorieDataService>();

        // Для ViewModels 
        builder.Services.AddTransient<ViewModels.MainPageViewModel>();
        builder.Services.AddTransient<ViewModels.EntryDetailViewModel>();

        builder.Services.AddTransient<Views.EntryDetailPage>();

        // Для профілю
        builder.Services.AddTransient<Views.UserProfileSetupPage>();
        builder.Services.AddSingleton<Services.CalorieCalculationService>();

        // Для фото 
        builder.Services.AddSingleton<Services.PhotoService>();

        // Сервіс конфігурації (читає appsettings.json)
        builder.Services.AddSingleton<Services.ConfigurationService>();
        // Сервіс розпізнавання їжі через Gemini AI
        builder.Services.AddSingleton<Services.GeminiVisionService>();

        return builder.Build();
    }
}
