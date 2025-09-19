using SQLite;
using TrackCalory.Models;

namespace TrackCalory.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly string _dbPath;

        public DatabaseService(string dbPath)
        {
            _dbPath = dbPath;
        }

        // Ініціалізація БД (створюється автоматично при першому виклику)
        private async Task InitAsync()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(_dbPath);

            // Створюємо таблицю CalorieEntries автоматично
            await _database.CreateTableAsync<CalorieEntry>();

            // Додаємо тестові дані, якщо БД пуста
            //await SeedDataIfEmptyAsync();
        }

        // ========== ОСНОВНІ ОПЕРАЦІЇ З БАЗОЮ ==========

        // Отримати всі записи (сортування: новіші спочатку)
        public async Task<List<CalorieEntry>> GetEntriesAsync()
        {
            await InitAsync();
            return await _database.Table<CalorieEntry>()
                                 .OrderByDescending(x => x.Date)
                                 .ToListAsync();
        }

        // Зберегти запис ( додати новий або оновити існуючий)
        public async Task<int> SaveEntryAsync(CalorieEntry entry)
        {
            await InitAsync();

            if (entry.Id != 0)
            {
                // Оновлюємо існуючий запис
                entry.UpdatedAt = DateTime.Now;
                return await _database.UpdateAsync(entry);
            }
            else
            {
                // Створюємо новий запис
                entry.CreatedAt = DateTime.Now;
                return await _database.InsertAsync(entry);
            }
        }

        // Видалити запис
        public async Task<int> DeleteEntryAsync(CalorieEntry entry)
        {
            await InitAsync();
            return await _database.DeleteAsync(entry);
        }

        // ========== СТАТИСТИКА ТА АНАЛІТИКА ==========

        // Калорії за конкретну дату
        public async Task<double> GetTotalCaloriesForDateAsync(DateTime date)
        {
            await InitAsync();
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            var entries = await _database.Table<CalorieEntry>()
                                        .Where(x => x.Date >= startDate && x.Date < endDate)
                                        .ToListAsync();

            return entries.Sum(x => x.Calories);
        }

        // Записи за конкретну дату
        public async Task<List<CalorieEntry>> GetEntriesByDateAsync(DateTime date)
        {
            await InitAsync();
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _database.Table<CalorieEntry>()
                                 .Where(x => x.Date >= startDate && x.Date < endDate)
                                 .OrderByDescending(x => x.Date)
                                 .ToListAsync();
        }

        // Статистика за тиждень
        public async Task<Dictionary<DateTime, double>> GetWeeklyStatisticsAsync()
        {
            await InitAsync();
            var weekAgo = DateTime.Today.AddDays(-7);

            var entries = await _database.Table<CalorieEntry>()
                                        .Where(x => x.Date >= weekAgo)
                                        .ToListAsync();

            return entries.GroupBy(x => x.Date.Date)
                         .ToDictionary(g => g.Key, g => g.Sum(x => x.Calories));
        }

        // ========== ДОПОМІЖНІ МЕТОДИ ==========

        public async Task<int> GetTotalEntriesCountAsync()
        {
            await InitAsync();
            return await _database.Table<CalorieEntry>().CountAsync();
        }

        // Очистити всі дані (для тестування)
        public async Task ClearAllDataAsync()
        {
            await InitAsync();
            await _database.DeleteAllAsync<CalorieEntry>();
        }

        /* Додати тестові дані, якщо БД пуста
        private async Task SeedDataIfEmptyAsync()
        {
            var count = await GetTotalEntriesCountAsync();

            if (count == 0)
            {
                var sampleEntries = new List<CalorieEntry>
                {
                    new CalorieEntry
                    {
                        Description = "Сніданок - вівсянка з ягодами",
                        Calories = 350,
                        Category = "Сніданок",
                        Date = DateTime.Today,
                        Protein = 12, Fat = 8, Carbs = 52
                    },
                    new CalorieEntry
                    {
                        Description = "Обід - курка з рисом",
                        Calories = 480,
                        Category = "Обід",
                        Date = DateTime.Today,
                        Protein = 35, Fat = 5, Carbs = 60
                    },
                    new CalorieEntry
                    {
                        Description = "Вечеря - салат з тунцем",
                        Calories = 320,
                        Category = "Вечеря",
                        Date = DateTime.Today.AddDays(-1),
                        Protein = 18, Fat = 24, Carbs = 15
                    }
                };

                foreach (var entry in sampleEntries)
                {
                    await SaveEntryAsync(entry);
                }
            }
        }*/
    }
}