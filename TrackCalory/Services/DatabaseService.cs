using SQLite;
using TrackCalory.Models;

namespace TrackCalory.Services
{
    /// <summary>
    /// DatabaseService — Сервіс прямої роботи з SQLite базою даних
    /// 
    /// Це найнижчий рівень доступу до даних. DatabaseService безпосередньо взаємодіє з БД,
    /// а CalorieDataService використовує DatabaseService для отримання/збереження даних
    /// та організує їх в ObservableCollection для UI.
    /// 
    /// ВІДПОВІДАЛЬНІСТЬ DatabaseService:
    /// - Управління підключенням до БД
    /// - операції з записами (Create, Read, Update, Delete)
    /// - Статистика та аналітика
    /// - Управління профілем користувача
    /// - Автоматичне створення таблиць
    /// 
    /// </summary>
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private readonly string _dbPath;

        public DatabaseService(string dbPath) // Автоматичне отримання шляху до БД при завантаженні сервісу (логіка шляху у App.xaml.cs)
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
            await _database.CreateTableAsync<UserProfile>();
        }

        // ========== ОСНОВНІ ОПЕРАЦІЇ З БАЗОЮ ==========

        /// <summary>
        /// Отримати всі записи (сортування: новіші спочатку)
        /// </summary>
        public async Task<List<CalorieEntry>> GetEntriesAsync()
        {
            await InitAsync();
            return await _database.Table<CalorieEntry>()
                                 .OrderByDescending(x => x.Date)
                                 .ToListAsync();
        }

        /// <summary>
        /// Зберегти запис ( додати новий або оновити існуючий)
        /// </summary>
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

        /// <summary>
        /// Видалити запис
        /// </summary>
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

        /// <summary>
        /// Отримати БЖВ за конкретну дату
        /// </summary>
        public async Task<(double protein, double fat, double carbs)> GetMacrosForDateAsync(DateTime date)
        {
            try
            {
                await InitAsync();

                var startDate = date.Date;
                var endDate = date.Date.AddDays(1);

                var entries = await _database.Table<CalorieEntry>()
                    .Where(e => e.Date >= startDate && e.Date < endDate)
                    .ToListAsync();

                double totalProtein = entries.Sum(e => e.Protein ?? 0);
                double totalFat = entries.Sum(e => e.Fat ?? 0);
                double totalCarbs = entries.Sum(e => e.Carbs ?? 0);

                return (totalProtein, totalFat, totalCarbs);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка отримання БЖВ: {ex.Message}");
                return (0, 0, 0);
            }
        }

        // ========== МЕТОДИ ДЛЯ РОБОТИ З ПРОФІЛЕМ КОРИСТУВАЧА ==========

        // Отримати профіль користувача
        public async Task<UserProfile> GetUserProfileAsync()
        {
            await InitAsync();
            return await _database.Table<UserProfile>().FirstOrDefaultAsync();
        }

        // Зберегти профіль користувача
        public async Task<int> SaveUserProfileAsync(UserProfile profile)
        {
            await InitAsync();

            profile.UpdatedAt = DateTime.Now;

            var existing = await GetUserProfileAsync();
            if (existing != null)
            {
                profile.Id = existing.Id;
                return await _database.UpdateAsync(profile);
            }
            else
            {
                profile.CreatedAt = DateTime.Now;
                return await _database.InsertAsync(profile);
            }
        }

        // Перевірити, чи існує профіль користувача
        public async Task<bool> HasUserProfileAsync()
        {
            await InitAsync();
            var count = await _database.Table<UserProfile>().CountAsync();
            return count > 0;
        }
    }
}