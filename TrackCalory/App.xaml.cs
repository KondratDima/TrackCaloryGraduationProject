using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Maui.Core;

namespace TrackCalory;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

        // ТЕСТ: Перевірка завантаження конфігурації
        try
        {
            var configService = new Services.ConfigurationService();
            bool hasKey = configService.IsApiKeyConfigured();
            System.Diagnostics.Debug.WriteLine($"✅ API ключ налаштований: {hasKey}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Помилка конфігурації: {ex.Message}");
        }


        MainPage = new AppShell();

        CheckUserProfile();

#if WINDOWS
        // Без застарілих методів
        Dispatcher.Dispatch(async () =>
        {
            await Task.Delay(500);
            if (Current?.Windows[0] is Window window)
            {
                window.Width = 540;
                window.Height = 960;
            }
        });
#endif

        
    }
    private async void CheckUserProfile()
    {
        try
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TrackCalory.db3");
            var databaseService = new Services.DatabaseService(dbPath);

            var hasProfile = await databaseService.HasUserProfileAsync();

            if (!hasProfile)
            {

                // Створюємо сторінку налаштування профілю
                var setupPage = new Views.UserProfileSetupPage(databaseService);
                // Створюємо NavigationPage, використовуючи створену сторінку
                var navigationPage = new NavigationPage(setupPage);
                // Встановлюємо рожевий колір для панелі навігації (заголовка)
                navigationPage.BarBackgroundColor = Color.FromArgb("#f4becb");
                // За бажанням, можна встановити колір тексту заголовка (наприклад, білий)
                navigationPage.BarTextColor = Colors.White;
                MainPage = navigationPage;

                /*
                // Якщо профілю немає - показуємо форму налаштування
                MainPage = new NavigationPage(new Views.UserProfileSetupPage(databaseService));
                */
            }
            else
            {
                // Якщо профіль є - показуємо головну сторінку
                MainPage = new AppShell();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Помилка перевірки профілю: {ex.Message}");
            MainPage = new AppShell();
        }
    }
    
}
