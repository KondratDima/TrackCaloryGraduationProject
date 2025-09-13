namespace TrackCalory;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();

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
}
