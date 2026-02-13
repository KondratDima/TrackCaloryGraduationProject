using SQLite;

namespace TrackCalory.Models
{
    // Це МОДЕЛЬ ТАБЛИЦЯ - описує, як виглядає профіль користувача
    [Table("UserProfile")]
    public class UserProfile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Gender { get; set; } // "Male" або "Female"

        public double Weight { get; set; } // вага в кг

        public double Height { get; set; } // зріст в см

        public int Age { get; set; } // вік в роках

        public string ActivityLevel { get; set; } // рівень активності

        public string Goal { get; set; } // мета: "maintain", "lose", "gain"

        // ГОЛОВНЕ ПОЛЕ - розрахована добова норма калорій
        public double DailyCalorieGoal { get; set; }

        // Денні норми БЖВ
        public double DailyProteinGoal { get; set; }
        public double DailyFatGoal { get; set; }
        public double DailyCarbsGoal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}