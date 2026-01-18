using System.Formats.Tar;
using TrackCalory.Models;
using TrackCalory.Services;
using TrackCalory.ViewModels;

namespace TrackCalory.Views;

public partial class AddEntryPage : ContentPage
{
    private readonly PhotoService _photoService;
    private readonly CalorieDataService _dataService;
    private string _currentPhotoPath;
    private readonly GeminiVisionService _geminiService;


    public AddEntryPage()
    {
        InitializeComponent();

        // Отримуємо сервіси
        _photoService = App.Current.Handler.MauiContext.Services.GetService<PhotoService>();

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "TrackCalory.db3");
        var databaseService = new DatabaseService(dbPath);
        _dataService = new CalorieDataService(databaseService);

        _geminiService = App.Current.Handler.MauiContext.Services.GetService<GeminiVisionService>();

        DatePicker.Date = DateTime.Today;
        Category.SelectedIndex = 0; // За замовчуванням "Сніданок"
    }

    /// <summary>
    /// ЗБЕРЕГТИ ЗАПИС
    /// </summary>
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
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
            
            // прибираємо фото з фотопревью
            PhotoPreviewFrame.IsVisible = false;
            PhotoPreview.Source = null;

            // Спочатку пробуємо закрити модальну сторінку
            if (Navigation.ModalStack.Count > 0)
            {
                await Navigation.PopModalAsync();
            }
            // Якщо це не модальна - закриваємо звичайну сторінку
            else if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
            // Якщо нічого не спрацювало - використовуємо Shell навігацію
            else
            {
                await Shell.Current.GoToAsync("//MainPage");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Помилка", $"Не вдалося зберегти запис: {ex.Message}", "OK");
        }
    }

    // ========= МЕТОДИ ЗВИЧАЙНОГО ЗАПОВНЕННЯ ЗАПИСУ ==========

    // Зробити фото через камеру
    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            // Використовуємо PermissionsHelper
            if (!await Services.PermissionsHelper.CheckAndRequestCameraPermissionAsync())
            {
                return;
            }

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
            // Використовуємо PermissionsHelper
            if (!await Services.PermissionsHelper.CheckAndRequestPhotosPermissionAsync())
            {
                return;
            }

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
            PhotoPreviewFrame.IsVisible = false;
            PhotoPreview.Source = null;
        }
        catch (Exception ex)
        {
            DisplayAlert("❌ Помилка", $"Не вдалося видалити фото: {ex.Message}", "OK");
        }
    }
    // перехід на MainPage
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
        // Якщо нічого не спрацювало - використовуємо Shell навігацію
        else
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }



    // ========= AI МЕТОДИ ЗАПОВНЕННЯ ЗАПИСУ ==========

    /// <summary> /// РОБИТЬ АНАЛІЗ ФОТО , ЗАПОВНЮЄ ПОЛЯ /// </summary>
    public async void OnAnalyzePhotoClicked(object sender, EventArgs e)
    {
        try
        {
            // КРОК 1: Запитуємо звідки взяти фото
            string action = await DisplayActionSheet(
                "🤖 Розпізнати калорії з AI",
                "Скасувати",
                null,
                "📷 Зробити фото зараз",
                "🖼️ Вибрати з галереї",
                _currentPhotoPath != null ? "✅ Використати поточне фото" : null);

            if (action == "Скасувати" || action == null)
                return;

            string photoPathToAnalyze = null;

            // КРОК 2:Отримуємо фото відповідно до вибору
            if (action == "📷 Зробити фото зараз")
            {
                //ПЕРЕВІРКА ДОЗВОЛІВ(Android 13 +)
                if (!await Services.PermissionsHelper.CheckAndRequestCameraPermissionAsync())
                {
                    return; 
                }

                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    photoPathToAnalyze = await SavePhotoAsync(photo);
                    await DisplayPhotoPreview(photoPathToAnalyze);
                }
            }
            else if (action == "🖼️ Вибрати з галереї")
            {
                // ПЕРЕВІРКА ДОЗВОЛІВ (Android 13+)
                if (!await Services.PermissionsHelper.CheckAndRequestPhotosPermissionAsync())
                {
                    return; 
                }

                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    photoPathToAnalyze = await SavePhotoAsync(photo);
                    await DisplayPhotoPreview(photoPathToAnalyze);
                }
            }
            else if (action == "✅ Використати поточне фото")
            {
                // Використовуємо вже завантажене фото
                photoPathToAnalyze = _currentPhotoPath;
            }

            // КРОК 3: Перевірка наявності фото
            if (string.IsNullOrEmpty(photoPathToAnalyze))
            {
                await DisplayAlert("⚠️ Помилка", "Не вдалося отримати фото для аналізу", "OK");
                return;
            }

            // ДУЖЕ ВАЖНО Чекаємо 100мс щоб Android завершив збереження стану після MediaPicker
            await Task.Delay(100);

            // КРОК 4: Показуємо індикатор завантаження
            var loadingPopup = new ContentPage
            {
                BackgroundColor = Color.FromRgba(0, 0, 0, 0.7),
                Content = new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 20,
                    Children =
                {
                    new ActivityIndicator
                    {
                        IsRunning = true,
                        Color = Colors.White,
                        WidthRequest = 60,
                        HeightRequest = 60
                    },
                    new Label
                    {
                        Text = "🤖 Аналізую фото через AI...",
                        TextColor = Colors.White,
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = "Це займе 5-15 секунд",
                        TextColor = Colors.LightGray,
                        FontSize = 14,
                        HorizontalOptions = LayoutOptions.Center
                    }
                }
                }
            };

            await Navigation.PushModalAsync(loadingPopup, false);

            try
            {
                // КРОК 5: Відправляємо фото на аналіз до Gemini
                System.Diagnostics.Debug.WriteLine($"🚀 Відправляємо фото на аналіз: {photoPathToAnalyze}");

                var result = await _geminiService.AnalyzeFoodFromPathAsync(photoPathToAnalyze);

                // Закриваємо індикатор
                await Navigation.PopModalAsync(false);

                // КРОК 6: Обробка результату
                if (result.IsValid)
                {
                    // ✅ УСПІХ - автоматично заповнюємо поля
                    DescriptionEntry.Text = result.DishName;
                    CaloriesEntry.Text = result.Calories.ToString("F0");

                    if (result.Protein.HasValue)
                        ProteinEntry.Text = result.Protein.Value.ToString("F1");

                    if (result.Fat.HasValue)
                        FatEntry.Text = result.Fat.Value.ToString("F1");

                    if (result.Carbs.HasValue)
                        CarbsEntry.Text = result.Carbs.Value.ToString("F1");

                    // Формуємо повідомлення для користувача
                    string message = $"✅ AI успішно розпізнав страву!\n\n";
                    message += $"🍽️ Страва: {result.DishName}\n";
                    message += $"🔥 Калорії: {result.Calories:F0} ккал\n";

                    if (result.Weight.HasValue)
                        message += $"⚖️ Вага: ~{result.Weight:F0} г\n";

                    if (result.Protein.HasValue || result.Fat.HasValue || result.Carbs.HasValue)
                    {
                        message += $"\n📊 БЖВ:\n";
                        message += $"  • Білки: {result.Protein:F1} г\n";
                        message += $"  • Жири: {result.Fat:F1} г\n";
                        message += $"  • Вуглеводи: {result.Carbs:F1} г\n";
                    }

                    if (result.Confidence.HasValue)
                    {
                        string confidenceEmoji = result.Confidence.Value >= 0.8 ? "🎯" :
                                                result.Confidence.Value >= 0.6 ? "⚠️" : "❓";
                        message += $"\n{confidenceEmoji} Впевненість AI: {result.Confidence:P0}";
                    }

                    message += "\n\n💡 Перевірте дані та збережіть запис.";

                    await DisplayAlert("🤖 AI Розпізнавання", message, "Добре");

                    System.Diagnostics.Debug.WriteLine($"✅ Розпізнано: {result.DishName}, {result.Calories} ккал");
                }
                else
                {
                    // ❌ ПОМИЛКА або не розпізнано
                    string errorMessage = result.Error ?? "Не вдалося розпізнати їжу на фото";

                    await DisplayAlert(
                        "⚠️ Не вдалося розпізнати",
                        $"{errorMessage}\n\n" +
                        "Можливі причини:\n" +
                        "• На фото немає їжі\n" +
                        "• Фото нечітке або темне\n" +
                        "• Перевищено ліміт API (15 запитів/хв)\n\n" +
                        "💡 Спробуйте зробити інше фото або введіть дані вручну.",
                        "OK");

                    System.Diagnostics.Debug.WriteLine($"❌ Не розпізнано: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                // Закриваємо індикатор у разі помилки
                await Navigation.PopModalAsync(false);

                await DisplayAlert(
                    "❌ Помилка",
                    $"Не вдалося проаналізувати фото:\n{ex.Message}\n\n" +
                    "Перевірте:\n" +
                    "• Інтернет-з'єднання\n" +
                    "• Налаштування API ключа\n" +
                    "• Якість фото",
                    "OK");

                System.Diagnostics.Debug.WriteLine($"❌ Exception в аналізі: {ex}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("❌ Помилка",
                $"Непередбачена помилка: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"❌ Outer exception: {ex}");
        }
    }
    // Тут при передачі фото з головної сторінки записується і запускається OnAnalyzePhotoFromMainPage
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Перевіряємо чи є фото для аналізу
        if (!string.IsNullOrEmpty(NavigationHelper.PendingPhotoPath))
        {
            var photoPath = NavigationHelper.PendingPhotoPath;
            NavigationHelper.PendingPhotoPath = null; // Очищаємо після використання

            await Task.Delay(200); // Даємо час на завантаження UI
            OnAnalyzePhotoFromMainPage(photoPath);
        }
    }
    public async void OnAnalyzePhotoFromMainPage(string photoPathToAnalyze)
    {
        try
        {
            // ДУЖЕ ВАЖНО Чекаємо 100мс щоб Android завершив збереження стану після MediaPicker
            await Task.Delay(100);
            await DisplayPhotoPreview(photoPathToAnalyze);

            // КРОК 4: Показуємо індикатор завантаження
            var loadingPopup = new ContentPage
            {
                BackgroundColor = Color.FromRgba(0, 0, 0, 0.7),
                Content = new VerticalStackLayout
                {
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    Spacing = 20,
                    Children =
                {
                    new ActivityIndicator
                    {
                        IsRunning = true,
                        Color = Colors.White,
                        WidthRequest = 60,
                        HeightRequest = 60
                    },
                    new Label
                    {
                        Text = "🤖 Аналізую фото через AI...",
                        TextColor = Colors.White,
                        FontSize = 20,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },
                    new Label
                    {
                        Text = "Це займе 5-15 секунд",
                        TextColor = Colors.LightGray,
                        FontSize = 14,
                        HorizontalOptions = LayoutOptions.Center
                    }
                }
                }
            };

            await Navigation.PushModalAsync(loadingPopup, false);

            try
            {
                // КРОК 5: Відправляємо фото на аналіз до Gemini
                System.Diagnostics.Debug.WriteLine($"🚀 Відправляємо фото на аналіз: {photoPathToAnalyze}");

                var result = await _geminiService.AnalyzeFoodFromPathAsync(photoPathToAnalyze);

                // Закриваємо індикатор
                await Navigation.PopModalAsync(false);

                // КРОК 6: Обробка результату
                if (result.IsValid)
                {
                    // УСПІХ - автоматично заповнюємо поля
                    DescriptionEntry.Text = result.DishName;
                    CaloriesEntry.Text = result.Calories.ToString("F0");

                    if (result.Protein.HasValue)
                        ProteinEntry.Text = result.Protein.Value.ToString("F1");

                    if (result.Fat.HasValue)
                        FatEntry.Text = result.Fat.Value.ToString("F1");

                    if (result.Carbs.HasValue)
                        CarbsEntry.Text = result.Carbs.Value.ToString("F1");

                    // Формуємо повідомлення для користувача
                    string message = $"✅ AI успішно розпізнав страву!\n\n";
                    message += $"🍽️ Страва: {result.DishName}\n";
                    message += $"🔥 Калорії: {result.Calories:F0} ккал\n";

                    if (result.Weight.HasValue)
                        message += $"⚖️ Вага: ~{result.Weight:F0} г\n";

                    if (result.Protein.HasValue || result.Fat.HasValue || result.Carbs.HasValue)
                    {
                        message += $"\n📊 БЖВ:\n";
                        message += $"  • Білки: {result.Protein:F1} г\n";
                        message += $"  • Жири: {result.Fat:F1} г\n";
                        message += $"  • Вуглеводи: {result.Carbs:F1} г\n";
                    }

                    if (result.Confidence.HasValue)
                    {
                        string confidenceEmoji = result.Confidence.Value >= 0.8 ? "🎯" :
                                                result.Confidence.Value >= 0.6 ? "⚠️" : "❓";
                        message += $"\n{confidenceEmoji} Впевненість AI: {result.Confidence:P0}";
                    }

                    message += "\n\n💡 Перевірте дані та збережіть запис.";

                    await DisplayAlert("🤖 AI Розпізнавання", message, "Добре");

                    System.Diagnostics.Debug.WriteLine($"✅ Розпізнано: {result.DishName}, {result.Calories} ккал");
                }
                else
                {
                    // ПОМИЛКА або не розпізнано
                    string errorMessage = result.Error ?? "Не вдалося розпізнати їжу на фото";

                    await DisplayAlert(
                        "⚠️ Не вдалося розпізнати",
                        $"{errorMessage}\n\n" +
                        "Можливі причини:\n" +
                        "• На фото немає їжі\n" +
                        "• Фото нечітке або темне\n" +
                        "• Перевищено ліміт API (15 запитів/хв)\n\n" +
                        "💡 Спробуйте зробити інше фото або введіть дані вручну.",
                        "OK");

                    System.Diagnostics.Debug.WriteLine($"❌ Не розпізнано: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                // Закриваємо індикатор у разі помилки
                await Navigation.PopModalAsync(false);

                await DisplayAlert(
                    "❌ Помилка",
                    $"Не вдалося проаналізувати фото:\n{ex.Message}\n\n" +
                    "Перевірте:\n" +
                    "• Інтернет-з'єднання\n" +
                    "• Налаштування API ключа\n" +
                    "• Якість фото",
                    "OK");

                System.Diagnostics.Debug.WriteLine($"❌ Exception в аналізі: {ex}");
            }
        }
        catch(Exception ex) {
            await DisplayAlert("❌ Помилка",
                $"Непередбачена помилка: {ex.Message}", "OK");
            System.Diagnostics.Debug.WriteLine($"❌ Outer exception: {ex}");
        }
    }


    // Допоміжний метод збереження фото 
    private async Task<string> SavePhotoAsync(FileResult photo)
    {
        if (photo == null) return null;

        try
        {
            var photosDirectory = Path.Combine(FileSystem.AppDataDirectory, "FoodPhotos");
            if (!Directory.Exists(photosDirectory))
            {
                Directory.CreateDirectory(photosDirectory);
            }

            string fileName = $"food_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            string filePath = Path.Combine(photosDirectory, fileName);

            using (var sourceStream = await photo.OpenReadAsync())
            using (var fileStream = File.Create(filePath))
            {
                await sourceStream.CopyToAsync(fileStream);
            }

            System.Diagnostics.Debug.WriteLine($"✅ Фото збережено: {filePath}");
            return filePath;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Помилка збереження: {ex}");
            return null;
        }
    }

    
}