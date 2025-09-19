using System;
using TrackCalory.Models;
using TrackCalory.Services;
using TrackCalory.ViewModels;
using TrackCalory.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace TrackCalory.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private CalorieDataService _dataService;
        private double _todayTotal;

        public MainPageViewModel()
        {
            InitializeServices();
        }

        // Ініціалізація сервісів (тимчасове рішення)
        private void InitializeServices()
        {
            try
            {
                // Отримуємо DatabaseService
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TrackCalory.db3");
                var databaseService = new DatabaseService(dbPath);

                // Створюємо CalorieDataService
                _dataService = new CalorieDataService(databaseService);

                Entries = _dataService.GetEntries();
                AddEntryCommand = new Command(async () => await AddEntry()); // визов сторінки додавання 
                DetailCommand = new Command<CalorieEntry>(async (entry) => await OpenEntryDetails(entry)); // визов сторінки деталей з зазначеним параметром з бази данних
                RefreshCommand = new Command(async () => await RefreshDataAsync());

                // Асинхронно завантажуємо дані
                _ = RefreshDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Помилка ініціалізації сервісів: {ex.Message}");
            }
        }

        public ObservableCollection<CalorieEntry> Entries { get; private set; }

        public double TodayTotal
        {
            get => _todayTotal;
            set
            {
                _todayTotal = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TodayTotalFormatted));
            }
        }

        // Форматований текст для відображення
        public string TodayTotalFormatted => $"Сьогодні: {TodayTotal:F0} ккал";

        public ICommand AddEntryCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }
        public ICommand DetailCommand { get; private set; }

        private async Task AddEntry()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new Views.AddEntryPage());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Помилка навігації: {ex.Message}");
            }
        }

        private async Task OpenEntryDetails(CalorieEntry entry)
        {
            try
            {
                if (entry == null) return;

                // Створюємо ViewModel для сторінки деталей з конкретним записом
                var detailViewModel = new EntryDetailViewModel(entry, _dataService);

                // Відкриваємо сторінку деталей
                var detailPage = new Views.EntryDetailPage(detailViewModel);
                await Application.Current.MainPage.Navigation.PushAsync(detailPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка відкриття деталей: {ex.Message}");
            }
        }

        // ОНОВЛЕНИЙ метод для асинхронного оновлення даних
        public async Task RefreshDataAsync()
        {
            try
            {
                if (_dataService == null) return;

                // Перезавантажуємо дані з БД
                await _dataService.LoadEntriesFromDatabaseAsync();

                // Оновлюємо загальну кількість калорій за сьогодні
                TodayTotal = await _dataService.GetTotalCaloriesForDateAsync(DateTime.Today);

                System.Diagnostics.Debug.WriteLine($" Дані оновлено. Калорій за сьогодні: {TodayTotal}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Помилка оновлення даних: {ex.Message}");
                TodayTotal = 0;
            }
        }

        // Метод для видалення запису 
        public async Task DeleteEntryAsync(CalorieEntry entry)
        {
            try
            {
                await _dataService.RemoveEntryAsync(entry);
                await RefreshDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Помилка видалення запису: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
