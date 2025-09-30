using SQLite;

namespace TrackCalory.Models
{
    [Table("UserProfile")]
    public class UserProfile
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Gender { get; set; } // "Male" ��� "Female"

        public double Weight { get; set; } // ���� � ��

        public double Height { get; set; } // ���� � ��

        public int Age { get; set; } // �� � �����

        public string ActivityLevel { get; set; } // ����� ���������

        public string Goal { get; set; } // ����: "maintain", "lose", "gain"

        // ������� ���� - ����������� ������ ����� ������
        public double DailyCalorieGoal { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}