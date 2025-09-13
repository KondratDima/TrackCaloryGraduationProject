using TrackCalory.ViewModels;

namespace TrackCalory
{

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Оновлюємо дані при поверненні на головну сторінку
            if (BindingContext is MainPageViewModel viewModel)
            {
                viewModel.RefreshData();
            }
        }
    }
}

