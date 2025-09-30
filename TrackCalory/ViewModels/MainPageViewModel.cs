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
        private DateTime _selectedDate;
        private double _selectedDateCalories;
        private ObservableCollection<CalorieEntry> _filteredEntries;

        public MainPageViewModel()
        {
            // Початково вибираємо сьогоднішню дату
            _selectedDate = DateTime.Today;
            _filteredEntries = new ObservableCollection<CalorieEntry>();

            InitializeServices();
        }

        private void InitializeServices()
        {
            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TrackCalory.db3");
                var databaseService = new DatabaseService(dbPath);
                _dataService = new CalorieDataService(databaseService);

                Entries = _dataService.GetEntries();

                // Команди навігації
                AddEntryCommand = new Command(async () => await AddEntry());
                DetailCommand = new Command<CalorieEntry>(async (entry) => await OpenEntryDetails(entry));
                RefreshCommand = new Command(async () => await RefreshDataAsync());

                // НОВІ КОМАНДИ для роботи з датами
                PreviousDayCommand = new Command(async () => await GoToPreviousDay());
                NextDayCommand = new Command(async () => await GoToNextDay());
                ShowDatePickerCommand = new Command(async () => await ShowDatePicker());

                // Завантажуємо дані для поточної дати
                _ = LoadDataForSelectedDateAsync();

                // Завантажуємо дані для добової калорійності
                _ = LoadUserProfileAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка ініціалізації сервісів: {ex.Message}");
            }
        }

        // ========== ВЛАСТИВОСТІ ==========

        public ObservableCollection<CalorieEntry> Entries { get; private set; }

        // НОВА КОЛЕКЦІЯ: записи тільки за вибрану дату
        public ObservableCollection<CalorieEntry> FilteredEntries
        {
            get => _filteredEntries;
            private set
            {
                _filteredEntries = value;
                OnPropertyChanged();
            }
        }

        // НОВІ ВЛАСТИВОСТІ для роботи з датами
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedDateFormatted));
                OnPropertyChanged(nameof(DateIndicator));
                _ = LoadDataForSelectedDateAsync();
            }
        }

        public string SelectedDateFormatted
        {
            get
            {
                /*if (SelectedDate.Date == DateTime.Today)
                    return "📅 Сьогодні";
                else if (SelectedDate.Date == DateTime.Today.AddDays(-1))
                    return "📅 Вчора";
                else if (SelectedDate.Date == DateTime.Today.AddDays(1))
                    return "📅 Завтра";
                else */
                return $"📅 {SelectedDate:dd.MM.yyyy}";
            }
        }

        public double SelectedDateCalories
        {
            get => _selectedDateCalories;
            set
            {
                _selectedDateCalories = value;
                OnPropertyChanged();
            }
        }

        public string DateIndicator
        {
            get
            {
                var daysDiff = (DateTime.Today - SelectedDate.Date).TotalDays;
                return daysDiff switch
                {
                    0 => "🟢 Сьогодні",
                    1 => "🔵 Вчора",
                    -1 => "🟡 Завтра",
                    > 1 => $"🔵 {daysDiff} днів тому",
                    < -1 => $"🟡 Через {Math.Abs(daysDiff)} днів",
                    _ => ""
                };
            }
        }

        // ========== КОМАНДИ ==========

        public ICommand AddEntryCommand { get; private set; }
        public ICommand DetailCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        // НОВІ КОМАНДИ
        public ICommand PreviousDayCommand { get; private set; }
        public ICommand NextDayCommand { get; private set; }
        public ICommand ShowDatePickerCommand { get; private set; }
        public ICommand GoToTodayCommand { get; private set; }
        public ICommand GoToYesterdayCommand { get; private set; }

        // ========== МЕТОДИ НАВІГАЦІЇ ПО ДАТАХ ==========

        private async Task GoToPreviousDay()
        {
            SelectedDate = SelectedDate.AddDays(-1);
        }

        private async Task GoToNextDay()
        {
            SelectedDate = SelectedDate.AddDays(1);
        }


        private async Task ShowDatePicker()
        {
            try
            {
                // Створюємо просту сторінку з DatePicker
                var datePicker = new DatePicker
                {
                    Date = SelectedDate,
                    Format = "dd.MM.yyyy",
                    FontSize = 18,
                    HorizontalOptions = LayoutOptions.Center
                };
                
                var page = new ContentPage
                {
                    Title = "Виберіть дату",
                    BackgroundColor = Color.FromArgb("#F5F5F5"),
                    Content = new StackLayout
                    {
                        Padding = 20,
                        Spacing = 20,
                        Children =
                        {
                             new Button
                             {
                                IsVisible = datePicker.Date != DateTime.Today,
                                Text = "Сьогодні",
                                FontSize = 22,
                                FontAttributes = FontAttributes.Bold,
                                BackgroundColor = Color.FromArgb("#5fd37c"),
                                TextColor = Color.FromArgb("#ffffff"),
                                CornerRadius = 15,
                                Padding = new Thickness(0, 15, 0, 15),
                                Command = new Command(async () =>
                                {
                                  SelectedDate = DateTime.Today;
                                  await Application.Current.MainPage.Navigation.PopAsync();
                                })
                             },
                            new Label
                            {
                                Text = "📅 Оберіть дату для перегляду:",
                                FontSize = 18,
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.Center
                            },
                            new Frame
                            {
                                BackgroundColor = Colors.White,
                                HasShadow = true,
                                CornerRadius = 10,
                                Padding = 20,
                                Content = datePicker
                            },
                            new Button
                            {
                                Text = "✅ Підтвердити",
                                BackgroundColor = Color.FromArgb("#98f1ae"),
                                TextColor = Colors.White,
                                FontSize = 16,
                                CornerRadius = 10,
                                Command = new Command(async () =>
                                {
                                    SelectedDate = datePicker.Date;
                                    await Application.Current.MainPage.Navigation.PopAsync();
                                })
                            },
                            new Button
                            {
                                Text = "❌ Скасувати",
                                BackgroundColor = Color.FromArgb("#a79599"),
                                TextColor = Colors.White,
                                FontSize = 16,
                                CornerRadius = 10,
                                Command = new Command(async () =>
                                {
                                    await Application.Current.MainPage.Navigation.PopAsync();
                                })
                            }
                        }
                    }
                };

                await Application.Current.MainPage.Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка показу DatePicker: {ex.Message}");
            }
        }

        // ========== ЗАВАНТАЖЕННЯ ДАНИХ ЗА ДАТОЮ ==========

        private async Task LoadDataForSelectedDateAsync()
        {
            try
            {
                if (_dataService == null) return;

                // Отримуємо записи за вибрану дату
                var entriesForDate = await _dataService.GetEntriesByDateAsync(SelectedDate);

                // Оновлюємо колекцію
                FilteredEntries.Clear();
                foreach (var entry in entriesForDate)
                {
                    FilteredEntries.Add(entry);
                }

                // Отримуємо загальну кількість калорій за дату
                SelectedDateCalories = await _dataService.GetTotalCaloriesForDateAsync(SelectedDate);

                // Оновлюємо добову норму колорій
                UpdateRemainingCalories();

                System.Diagnostics.Debug.WriteLine($"✅ Завантажено {FilteredEntries.Count} записів за {SelectedDate:dd.MM.yyyy}. Калорій: {SelectedDateCalories}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка завантаження даних за дату: {ex.Message}");
                SelectedDateCalories = 0;
            }
        }
        // ========== ДОБОВА КОЛОРІЙНІСТЬ ==========
        private double _dailyGoal;
        private double _remainingCalories;

        public double DailyGoal
        {
            get => _dailyGoal;
            set
            {
                _dailyGoal = value;
                OnPropertyChanged();
                UpdateRemainingCalories();
            }
        }

        public double RemainingCalories
        {
            get => _remainingCalories;
            set
            {
                _remainingCalories = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(RemainingCaloriesFormatted));
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }

        // Повертає str який пише скільки залишилося набрати колорій за день та чи перевищено
        // ПОКИ НЕ ВИКОРИСТОВУЄТЬСЯ
        public string RemainingCaloriesFormatted
        {
            get
            {
                if (RemainingCalories > 0)
                    return $"Залишилось: {RemainingCalories:F0} ккал";
                else if (RemainingCalories == 0)
                    return "Норма досягнута! 🎯";
                else
                    return $"Перевищено на {Math.Abs(RemainingCalories):F0} ккал ⚠️";
            }
        }

        public double ProgressPercentage
        {
            get
            {
                if (DailyGoal == 0) return 0;
                return (SelectedDateCalories / DailyGoal);
            }
        }

        private void UpdateRemainingCalories()
        {
            RemainingCalories = DailyGoal - SelectedDateCalories;
        }

        private async Task LoadUserProfileAsync()
        {
            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TrackCalory.db3");
                var databaseService = new DatabaseService(dbPath);

                var profile = await databaseService.GetUserProfileAsync();
                if (profile != null)
                {
                    DailyGoal = profile.DailyCalorieGoal;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка завантаження профілю: {ex.Message}");
            }
        }

        // ========== ІСНУЮЧІ МЕТОДИ ==========

        private async Task AddEntry()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new Views.AddEntryPage());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка навігації: {ex.Message}");
            }
        }

        private async Task OpenEntryDetails(CalorieEntry entry)
        {
            try
            {
                if (entry == null) return;
                var detailViewModel = new EntryDetailViewModel(entry, _dataService);
                var detailPage = new Views.EntryDetailPage(detailViewModel);
                await Application.Current.MainPage.Navigation.PushAsync(detailPage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка відкриття деталей: {ex.Message}");
            }
        }

        public async Task RefreshDataAsync()
        {
            try
            {
                if (_dataService == null) return;

                // Перезавантажуємо всі дані
                await _dataService.LoadEntriesFromDatabaseAsync();

                // Оновлюємо дані за поточну дату
                await LoadDataForSelectedDateAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Дані оновлено для {SelectedDate:dd.MM.yyyy}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка оновлення даних: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
