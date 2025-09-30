using TrackCalory.Models;

namespace TrackCalory.Services
{
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
    }
}