using TrackCalory.ViewModels;

namespace TrackCalory.Views;

public partial class EntryDetailPage : ContentPage
{
    public EntryDetailPage(EntryDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}