namespace TrackCalory.Views;

public partial class Info : ContentPage
{
	public Info()
	{
		InitializeComponent();
	}
    private async void OnLinkClicked(object sender, EventArgs e)
    {
        await Launcher.Default.OpenAsync("https://github.com/KondratDima/TrackCaloryGraduationProject");
    }
}