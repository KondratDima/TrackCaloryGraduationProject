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
                await Task.Delay(100);
                //await viewModel.RefreshDataAsync();
                await Task.Delay(50);
                this.ForceLayout();
            }
        }
    }
}

