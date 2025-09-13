using System;
using TrackCalory.Models;
using System.Collections.ObjectModel;

namespace TrackCalory.Services
{
    public class CalorieDataService
    {
        private static CalorieDataService _instance;
        private ObservableCollection<CalorieEntry> _entries;

        public static CalorieDataService Instance => _instance ??= new CalorieDataService();

        private CalorieDataService()
        {
            _entries = new ObservableCollection<CalorieEntry>();
            LoadSampleData(); // Тимчасові дані для тестування
        }

        public ObservableCollection<CalorieEntry> GetEntries() => _entries;

        public void AddEntry(CalorieEntry entry)
        {
            entry.Id = _entries.Count + 1;
            _entries.Insert(0, entry); // Додаємо на початок (новіші записи зверху)
        }

        public void RemoveEntry(CalorieEntry entry)
        {
            _entries.Remove(entry);
        }

        public double GetTotalCaloriesForDate(DateTime date)
        {
            return _entries.Where(e => e.Date.Date == date.Date).Sum(e => e.Calories);
        }

        public Dictionary<DateTime, double> GetDailySummary()
        {
            return _entries
                .GroupBy(e => e.Date.Date)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Calories));
        }

        private void LoadSampleData()
        {
            // Тимчасові дані для демонстрації
            _entries.Add(new CalorieEntry
            {
                Id = 1,
                Date = DateTime.Today,
                Calories = 350,
                Description = "Сніданок - вівсянка з ягодами"
            });
            _entries.Add(new CalorieEntry
            {
                Id = 2,
                Date = DateTime.Today,
                Calories = 480,
                Description = "Обід - курка з рисом"
            });
            _entries.Add(new CalorieEntry
            {
                Id = 3,
                Date = DateTime.Today.AddDays(-1),
                Calories = 320,
                Description = "Вечеря - салат з тунцем"
            });
        }
    }
}
