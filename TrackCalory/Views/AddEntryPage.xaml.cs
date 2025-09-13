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
                Description = DescriptionEntry.Text,
                Calories = calories,
                Date = DatePicker.Date
            };

            CalorieDataService.Instance.AddEntry(entry);
            DescriptionEntry.Text = "";
            CaloriesEntry.Text = "";

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