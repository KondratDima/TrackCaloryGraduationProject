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
                await DisplayAlert("�������", "���� �����, ������ ���� ������", "OK");
                return;
            }

            if (!double.TryParse(CaloriesEntry.Text, out double calories) || calories <= 0)
            {
                await DisplayAlert("�������", "���� �����, ������ ��������� ������� ������", "OK");
                return;
            }

            var entry = new CalorieEntry
            {
                Description = DescriptionEntry.Text.Trim(),
                Calories = calories,
                Date = DatePicker.Date,
                Category = "������� ������"
            };

            // �������� ����� ����� �� �������� ����� � ��
            var dataService = Handler.MauiContext.Services.GetService<CalorieDataService>();
            await dataService.AddEntryAsync(entry);

            await DisplayAlert("����! ",$"����� ���������:\n{entry.Description}\n{entry.Calories:F0} ����", "�����");

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("�������", $"�� ������� �������� �����: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}