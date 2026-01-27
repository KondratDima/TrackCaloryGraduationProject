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

    /// <summary>
    /// Кнопка розрахунку та збереження профілю користувача 
    /// </summary>
    private async void OnCalculateClicked(object sender, EventArgs e)
    {
        try
        {
            // Валідація
            if (GenderPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Помилка", "Оберіть стать", "OK");
                return;
            }

            if (!double.TryParse(WeightEntry.Text, out double weight) || weight <= 0)
            {
                await DisplayAlert("Помилка", "Введіть правильну вагу", "OK");
                return;
            }

            if (!double.TryParse(HeightEntry.Text, out double height) || height <= 0)
            {
                await DisplayAlert("Помилка", "Введіть правильний зріст", "OK");
                return;
            }

            if (!int.TryParse(AgeEntry.Text, out int age) || age <= 0)
            {
                await DisplayAlert("Помилка", "Введіть правильний вік", "OK");
                return;
            }

            if (ActivityPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Помилка", "Оберіть рівень активності", "OK");
                return;
            }

            if (GoalPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Помилка", "Оберіть мету", "OK");
                return;
            }

            // Конвертація значень
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

            // Створюємо профіль
            var profile = new UserProfile
            {
                Gender = gender,
                Weight = weight,
                Height = height,
                Age = age,
                ActivityLevel = activityLevel,
                Goal = goal
            };

            // Розраховуємо денну норму
            profile.DailyCalorieGoal = _calculationService.CalculateFullGoal(profile);

            // Зберігаємо в БД
            await _databaseService.SaveUserProfileAsync(profile);

            // Показуємо результат
            await DisplayAlert(" Успіх!",
                $"Ваша денна норма калорій: {profile.DailyCalorieGoal:F0} ккал\n\n" +
                $"Профіль збережено!",
                "Почати");

            // Переходимо на головну сторінку
            Application.Current.MainPage = new AppShell();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося зберегти профіль:\n{ex.Message}", "OK");
        }
    }
}