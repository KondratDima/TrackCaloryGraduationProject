using Microsoft.Extensions.Logging;

namespace TrackCalory;

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

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // ========== ДОДАТИ РЕЄСТРАЦІЮ СЕРВІСІВ ==========

        // Визначаємо шлях до БД
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TrackCalory.db3");

        // Реєструємо DatabaseService з конкретним шляхом до БД
        builder.Services.AddSingleton(s => new Services.DatabaseService(dbPath));

        // Реєструємо CalorieDataService з залежністю від DatabaseService
        builder.Services.AddSingleton<Services.CalorieDataService>();

        // Для ViewModels (якщо будете використовувати DI)
        builder.Services.AddTransient<ViewModels.MainPageViewModel>();

        return builder.Build();
    }
}
