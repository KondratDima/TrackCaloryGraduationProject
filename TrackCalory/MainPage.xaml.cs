using TrackCalory.ViewModels;

namespace TrackCalory
{

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Асинхронно оновлюємо дані при поверненні на головну сторінку
            if (BindingContext is MainPageViewModel viewModel)
            {
                await viewModel.RefreshDataAsync();
            }
        }
    }
}

