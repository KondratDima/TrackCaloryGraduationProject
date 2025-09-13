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
            _dataService = CalorieDataService.Instance;
            Entries = _dataService.GetEntries();
            AddEntryCommand = new Command(async () => await AddEntry()); // {Command відкриття AddEntryPage з постійним оновленням}

            RefreshData();
        }

        public ObservableCollection<CalorieEntry> Entries { get; }

        public double TodayTotal
        {
            get => _todayTotal;
            set
            {
                _todayTotal = value;
                OnPropertyChanged();
            }
        }

        public ICommand AddEntryCommand { get; } // {Command відкриття AddEntryPage}
        private async Task AddEntry()
        {
            // Переходимо на сторінку додавання нового запису
            //await Shell.Current.GoToAsync("//AddEntryPage"); // Другий спосіб (Shell Navigation)
            await Application.Current.MainPage.Navigation.PushAsync(new Views.AddEntryPage());
        }

        public void RefreshData()
        {
            TodayTotal = _dataService.GetTotalCaloriesForDate(DateTime.Today);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
