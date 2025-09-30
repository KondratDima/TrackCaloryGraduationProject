using TrackCalory.Models;

namespace TrackCalory.Services
{
    public class CalorieCalculationService
    {
        // ���������� BMR �� �������� ̳�����-��� �����
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

        // ���������� TDEE (� ����������� ���������)
        public double CalculateTDEE(double bmr, string activityLevel)
        {
            var multiplier = activityLevel switch
            {
                "Sedentary" => 1.2,        // �������
                "Light" => 1.375,          // ����� ���������
                "Moderate" => 1.55,        // ������ ���������
                "Active" => 1.725,         // ��������
                "VeryActive" => 1.9,       // ���� ��������
                _ => 1.2
            };

            return bmr * multiplier;
        }

        // ���������� ����� ����� � ����������� ����
        public double CalculateDailyGoal(double tdee, string goal)
        {
            return goal switch
            {
                "lose" => tdee - 400,      // ���������: -400 ����
                "gain" => tdee + 400,      // ���� ����: +400 ����
                _ => tdee                  // ϳ�������: ��� ���
            };
        }

        // ������ ����������
        public double CalculateFullGoal(UserProfile profile)
        {
            var bmr = CalculateBMR(profile.Gender, profile.Weight, profile.Height, profile.Age);
            var tdee = CalculateTDEE(bmr, profile.ActivityLevel);
            return CalculateDailyGoal(tdee, profile.Goal);
        }
    }
}