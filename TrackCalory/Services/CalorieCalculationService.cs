using TrackCalory.Models;

namespace TrackCalory.Services
{
    /// <summary>
    /// Розраховує денну норму калорій на основі профілю користувача
    /// </summary>
    public class CalorieCalculationService
    {
        // Розрахунок BMR за формулою Міффліна-Сен Жеора
        public double CalculateBMR(string gender, double weight, double height, int age)
        {
            if (gender == "Male")
            {
                return (10 * weight) + (6.25 * height) - (5 * age) + 5;
            }
            else // Female
            {
                return (10 * weight) + (6.25 * height) - (5 * age) - 161;
            }
        }

        // Розрахунок TDEE (з урахуванням активності)
        public double CalculateTDEE(double bmr, string activityLevel)
        {
            var multiplier = activityLevel switch
            {
                "Sedentary" => 1.2,        // Сидячий
                "Light" => 1.375,          // Легка активність
                "Moderate" => 1.55,        // Помірна активність
                "Active" => 1.725,         // Активний
                "VeryActive" => 1.9,       // Дуже активний
                _ => 1.2
            };

            return bmr * multiplier;
        }

        // Розрахунок денної норми з урахуванням мети
        public double CalculateDailyGoal(double tdee, string goal)
        {
            return goal switch
            {
                "lose" => tdee - 400,      // Схуднення: -400 ккал
                "gain" => tdee + 400,      // Набір маси: +400 ккал
                _ => tdee                  // Підтримка: без змін
            };
        }

        // Повний розрахунок
        public double CalculateFullGoal(UserProfile profile)
        {
            var bmr = CalculateBMR(profile.Gender, profile.Weight, profile.Height, profile.Age);
            var tdee = CalculateTDEE(bmr, profile.ActivityLevel);
            return CalculateDailyGoal(tdee, profile.Goal);
        }

        /// ============ БЖВ ============
        /// <summary>
        /// Розрахунок денної норми білків (г)
        /// Стандарт: 1.5-2г на кг ваги для активних, 1г для сидячих
        /// </summary>
        public double CalculateDailyProtein(double weight, string activityLevel, string goal)
        {
            double proteinPerKg = activityLevel switch
            {
                "Сидячий спосіб життя" => 1.0,
                "Легка активність (1-3 дні/тиждень)" => 1.2,
                "Помірна активність (3-5 днів/тиждень)" => 1.5,
                "Активний (6-7 днів/тиждень)" => 1.8,
                "Дуже активний (фізична робота)" => 2.0,
                _ => 1.2
            };

            // Для набору м'язів - більше білка
            if (goal == "gain")
                proteinPerKg += 0.3;

            return weight * proteinPerKg;
        }

        /// <summary>
        /// Розрахунок денної норми жирів (г)
        /// Стандарт: 25-30% від загальних калорій (1г жиру = 9 ккал)
        /// </summary>
        public double CalculateDailyFat(double dailyCalories)
        {
            // 25% калорій з жирів
            double fatCalories = dailyCalories * 0.25;
            return fatCalories / 9; // 1г жиру = 9 ккал
        }

        /// <summary>
        /// Розрахунок денної норми вуглеводів (г)
        /// Залишок калорій після білків і жирів (1г вуглеводів = 4 ккал)
        /// </summary>
        public double CalculateDailyCarbs(double dailyCalories, double dailyProtein, double dailyFat)
        {
            // Калорії з білків і жирів
            double proteinCalories = dailyProtein * 4; // 1г білка = 4 ккал
            double fatCalories = dailyFat * 9; // 1г жиру = 9 ккал

            // Залишок іде на вуглеводи
            double carbsCalories = dailyCalories - proteinCalories - fatCalories;
            return carbsCalories / 4; // 1г вуглеводів = 4 ккал
        }
    }
}