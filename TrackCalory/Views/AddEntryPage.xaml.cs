using System.Formats.Tar;
using TrackCalory.Models;
using TrackCalory.Services;

namespace TrackCalory.Views;

public partial class AddEntryPage : ContentPage
{
    private readonly PhotoService _photoService;
    private readonly CalorieDataService _dataService;
    private string _currentPhotoPath;


    public AddEntryPage()
    {
        InitializeComponent();

        // Отримуємо сервіси
        _photoService = App.Current.Handler.MauiContext.Services.GetService<PhotoService>();

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TrackCalory.db3");
        var databaseService = new DatabaseService(dbPath);
        _dataService = new CalorieDataService(databaseService);

        DatePicker.Date = DateTime.Today;
        Category.SelectedIndex = 0; // За замовчуванням "Сніданок"
    }

    /// <summary>
    /// Зберегти запис
    /// </summary>
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            /*if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
            {
                await DisplayAlert("Помилка", "Будь ласка, введіть опис страви", "OK");
                return;
            }

            if (!double.TryParse(CaloriesEntry.Text, out double calories) || calories <= 0)
            {
                await DisplayAlert("Помилка", "Будь ласка, введіть правильну кількість калорій", "OK");
                return;
            }
            if (Category.SelectedIndex == -1)
            {
                await DisplayAlert("Помилка", "Оберіть тип", "OK");
                return;
            }

            var entry = new CalorieEntry
            {
                Description = DescriptionEntry.Text.Trim(),
                Calories = calories,
                Date = DatePicker.Date,
                Category = Category.SelectedIndex switch
                {
                    0 => "Сніданок",
                    1 => "Обід",
                    2 => "Вечеря",
                    3 => "Перекус",
                    4 => "Десерт",
                    5 => "Напій",
                    6 => "Інше"
                }
            };*/
            // Валідація
            if (string.IsNullOrWhiteSpace(DescriptionEntry.Text))
            {
                await DisplayAlert("⚠️ Помилка", "Введіть опис страви", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(CaloriesEntry.Text) ||
                !double.TryParse(CaloriesEntry.Text, out double calories))
            {
                await DisplayAlert("⚠️ Помилка", "Введіть коректну кількість калорій", "OK");
                return;
            }

            // Парсимо БЖУ (необов'язкові)
            double? protein = double.TryParse(ProteinEntry.Text, out double p) ? p : null;
            double? fat = double.TryParse(FatEntry.Text, out double f) ? f : null;
            double? carbs = double.TryParse(CarbsEntry.Text, out double c) ? c : null;

            // Отримуємо категорію
            string category = Category.SelectedItem?.ToString() ?? "Інше";

            // Створюємо новий запис
            var entry = new CalorieEntry
            {
                Description = DescriptionEntry.Text.Trim(),
                Calories = calories,
                Date = DatePicker.Date.Add(DateTime.Now.TimeOfDay), // Зберігаємо час
                Category = category,
                Protein = protein,
                Fat = fat,
                Carbs = carbs,
                PhotoPath = _currentPhotoPath, // Зберігаємо шлях до фото
                CreatedAt = DateTime.Now
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

    // Зробити фото через камеру
    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photoPath = await _photoService.TakePhotoAsync();

            if (!string.IsNullOrEmpty(photoPath))
            {
                await DisplayPhotoPreview(photoPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Помилка", $"Не вдалося зробити фото: {ex.Message}", "OK");
        }
    }

    // Вибрати фото з галереї
    private async void OnPickPhotoClicked(object sender, EventArgs e)
    {
        try
        {
            var photoPath = await _photoService.PickPhotoAsync();

            if (!string.IsNullOrEmpty(photoPath))
            {
                await DisplayPhotoPreview(photoPath);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Помилка", $"Не вдалося вибрати фото: {ex.Message}", "OK");
        }
    }

    // Показати прев'ю фото
    private async Task DisplayPhotoPreview(string photoPath)
    {
        try
        {
            // Видаляємо попереднє фото, якщо було
            if (!string.IsNullOrEmpty(_currentPhotoPath) && _currentPhotoPath != photoPath)
            {
                _photoService.DeletePhoto(_currentPhotoPath);
            }

            _currentPhotoPath = photoPath;

            // Показуємо прев'ю
            PhotoPreview.Source = ImageSource.FromFile(photoPath);
            PhotoPreviewFrame.IsVisible = true;

            System.Diagnostics.Debug.WriteLine($"✅ Фото встановлено: {photoPath}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Помилка", $"Не вдалося показати фото: {ex.Message}", "OK");
        }
    }

    // Видалити фото
    private void OnRemovePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            if (!string.IsNullOrEmpty(_currentPhotoPath))
            {
                _photoService.DeletePhoto(_currentPhotoPath);
                _currentPhotoPath = null;
            }

            PhotoPreviewFrame.IsVisible = false;
            PhotoPreview.Source = null;
        }
        catch (Exception ex)
        {
            DisplayAlert("❌ Помилка", $"Не вдалося видалити фото: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}