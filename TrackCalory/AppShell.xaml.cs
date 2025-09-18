namespace TrackCalory;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
        Routing.RegisterRoute(nameof(Views.EntryDetailPage), typeof(Views.EntryDetailPage));
    }
}
