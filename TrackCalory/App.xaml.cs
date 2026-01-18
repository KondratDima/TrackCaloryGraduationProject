using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Maui.Core;

namespace TrackCalory;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
        // примусово світла тема
        Application.Current.UserAppTheme = AppTheme.Light;
        this.RequestedThemeChanged += (s, e) => { Application.Current.UserAppTheme = AppTheme.Light; };

#if WINDOWS
        // Примусово вмикаємо світлу тему для WinUI (нативна частина — title bar тощо)
        try
        {
            Microsoft.UI.Xaml.Application.Current.RequestedTheme = Microsoft.UI.Xaml.ApplicationTheme.Light;
        }
        catch
        {
            // Якщо з якоїсь причини не доступно — ігноруємо без падіння
        }
#endif

        MainPage = new AppShell();

        CheckUserProfile();

#if WINDOWS
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
                // Встановлюємо рожевий колір для панелі навігації
                navigationPage.BarBackgroundColor = Color.FromArgb("#f4becb");
                // колір тексту заголовка 
                navigationPage.BarTextColor = Colors.Black;
                MainPage = navigationPage;
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
