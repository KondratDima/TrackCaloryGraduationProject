using System;
using TrackCalory.Models;
using TrackCalory.Services;
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
            // ТИМЧАСОВЕ РІШЕННЯ: отримуємо сервіс через ServiceLocator
            // (краще використовувати Dependency Injection, але це простіше для початку)
            InitializeServices();
        }

        // Конструктор з Dependency Injection (для майбутнього використання) . Знайшов такий метод але не розумію як його використати
        /*
        public MainPageViewModel(CalorieDataService dataService)
        {
            _dataService = dataService;
            Entries = _dataService.GetEntries();
            AddEntryCommand = new Command(async () => await AddEntry());
            RefreshCommand = new Command(async () => await RefreshDataAsync());

            // Асинхронно завантажуємо дані
            _ = RefreshDataAsync();
        }
        */

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
                DetailCommand = new Command(async () => await Detail()); // визов сторінки деталей 
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

        private async Task Detail()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new Views.EntryDetailPage());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Помилка навігації: {ex.Message}");
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

        /* ЗАСТАРІЛИЙ синхронний метод (для сумісності)
        public void RefreshData()
        {
            _ = RefreshDataAsync();
        }
        */

        // Метод для видалення запису (для майбутнього використання)
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
