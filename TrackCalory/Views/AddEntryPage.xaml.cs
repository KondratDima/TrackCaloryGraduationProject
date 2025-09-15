using TrackCalory.Models;
using TrackCalory.Services;

namespace TrackCalory.Views;

public partial class AddEntryPage : ContentPage
{


    public AddEntryPage()
    {
        InitializeComponent();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
            {
                await DisplayAlert("Помилка", "Будь ласка, введіть опис страви", "OK");
                return;
            }

            if (!double.TryParse(CaloriesEntry.Text, out double calories) || calories <= 0)
            {
                await DisplayAlert("Помилка", "Будь ласка, введіть правильну кількість калорій", "OK");
                return;
            }

            var entry = new CalorieEntry
            {
                Description = DescriptionEntry.Text.Trim(),
                Calories = calories,
                Date = DatePicker.Date,
                Category = "Основна страва"
            };

            // Отримуємо сервіс даних та зберігаємо запис В БД
            var dataService = Handler.MauiContext.Services.GetService<CalorieDataService>();
            await dataService.AddEntryAsync(entry);

            await DisplayAlert("Успіх! ",$"Запис збережено:\n{entry.Description}\n{entry.Calories:F0} ккал", "Добре");

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося зберегти запис: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}