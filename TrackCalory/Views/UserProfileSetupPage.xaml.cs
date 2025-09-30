using TrackCalory.Models;
using TrackCalory.Services;

namespace TrackCalory.Views;

public partial class UserProfileSetupPage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly CalorieCalculationService _calculationService;

    public UserProfileSetupPage(DatabaseService databaseService)
    {
        InitializeComponent();
        _databaseService = databaseService;
        _calculationService = new CalorieCalculationService();
    }

    private async void OnCalculateClicked(object sender, EventArgs e)
    {
        try
        {
            // ��������
            if (GenderPicker.SelectedIndex == -1)
            {
                await DisplayAlert("�������", "������ �����", "OK");
                return;
            }

            if (!double.TryParse(WeightEntry.Text, out double weight) || weight <= 0)
            {
                await DisplayAlert("�������", "������ ��������� ����", "OK");
                return;
            }

            if (!double.TryParse(HeightEntry.Text, out double height) || height <= 0)
            {
                await DisplayAlert("�������", "������ ���������� ����", "OK");
                return;
            }

            if (!int.TryParse(AgeEntry.Text, out int age) || age <= 0)
            {
                await DisplayAlert("�������", "������ ���������� ��", "OK");
                return;
            }

            if (ActivityPicker.SelectedIndex == -1)
            {
                await DisplayAlert("�������", "������ ����� ���������", "OK");
                return;
            }

            if (GoalPicker.SelectedIndex == -1)
            {
                await DisplayAlert("�������", "������ ����", "OK");
                return;
            }

            // ����������� �������
            var gender = GenderPicker.SelectedIndex == 0 ? "Male" : "Female";

            var activityLevel = ActivityPicker.SelectedIndex switch
            {
                0 => "Sedentary",
                1 => "Light",
                2 => "Moderate",
                3 => "Active",
                4 => "VeryActive",
                _ => "Sedentary"
            };

            var goal = GoalPicker.SelectedIndex switch
            {
                0 => "lose",
                1 => "maintain",
                2 => "gain",
                _ => "maintain"
            };

            // ��������� �������
            var profile = new UserProfile
            {
                Gender = gender,
                Weight = weight,
                Height = height,
                Age = age,
                ActivityLevel = activityLevel,
                Goal = goal
            };

            // ����������� ����� �����
            profile.DailyCalorieGoal = _calculationService.CalculateFullGoal(profile);

            // �������� � ��
            await _databaseService.SaveUserProfileAsync(profile);

            // �������� ���������
            await DisplayAlert(" ����!",
                $"���� ����� ����� ������: {profile.DailyCalorieGoal:F0} ����\n\n" +
                $"������� ���������!",
                "������");

            // ���������� �� ������� �������
            Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            await DisplayAlert("�������", $"�� ������� �������� �������:\n{ex.Message}", "OK");
        }
    }
}