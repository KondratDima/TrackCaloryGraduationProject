using System;
using TrackCalory.Models;
using System.Collections.ObjectModel;

namespace TrackCalory.Services
{
    /// <summary>
    /// Проміжний шар між UI (Views/ViewModels) та базою даних. 
    /// 
    /// Працює разом з DatabaseService підгружаючи данні в ObservableCollection
    /// 
    /// Забезпечує роботу з записами про їжу, включаючи :
    /// - Додавання записів
    /// - видалення записів
    /// - отримання статистики
    /// - пошук записів за датою
    /// </summary>
    public class CalorieDataService
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<CalorieEntry> _entries;

        public CalorieDataService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            _entries = new ObservableCollection<CalorieEntry>();

            // Завантажуємо дані з БД при створенні сервісу
            _ = LoadEntriesFromDatabaseAsync();
        }

        // ГОЛОВНА КОЛЕКЦІЯ ЗАПИСІВ - потім використовується ViewModel-ами 
        public ObservableCollection<CalorieEntry> GetEntries() => _entries;

        /// <summary>
        /// Завантажити записи з БД в ObservableCollection
        /// </summary>
        public async Task LoadEntriesFromDatabaseAsync()
        {
            try
            {
                var entries = await _databaseService.GetEntriesAsync();

                _entries.Clear();
                foreach (var entry in entries)
                {
                    _entries.Add(entry);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Помилка завантаження даних: {ex.Message}");
            }
        }

        /// <summary>
        /// Метод для отримання записів за певну дату
        /// </summary>
        public async Task<List<CalorieEntry>> GetEntriesByDateAsync(DateTime date)
        {
            try
            {
                return await _databaseService.GetEntriesByDateAsync(date);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка отримання записів за дату: {ex.Message}");
                return new List<CalorieEntry>();
            }
        }

        /// <summary>
        /// Додати новий запис
        /// </summary>
        public async Task AddEntryAsync(CalorieEntry entry)
        {
            try
            {
                // Зберігаємо в БД
                await _databaseService.SaveEntryAsync(entry);

                // Додаємо до колекції на початок (новіші записи зверху)
                _entries.Insert(0, entry);

                System.Diagnostics.Debug.WriteLine($" Запис додано: {entry.Description}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Помилка додавання запису: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Видалити запис
        /// </summary>
        public async Task RemoveEntryAsync(CalorieEntry entry)
        {
            try
            {
                await _databaseService.DeleteEntryAsync(entry);
                _entries.Remove(entry);

                System.Diagnostics.Debug.WriteLine($" Запис видалено: {entry.Description}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Помилка видалення запису: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Калорії за сьогодні (через БД)
        /// </summary>
        public async Task<double> GetTotalCaloriesForDateAsync(DateTime date)
        {
            try
            {
                return await _databaseService.GetTotalCaloriesForDateAsync(date);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" Помилка отримання калорій: {ex.Message}");
                return 0;
            }
        }

    }
}
