using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using TrackCalory.Models;
using TrackCalory.Services;

namespace TrackCalory.ViewModels
{
    /// <summary>
    /// ViewModel для детального перегляду запису про калорії та його видалення
    /// </summary>
    public class EntryDetailViewModel : INotifyPropertyChanged
    {
        private readonly CalorieEntry _entry;
        private readonly CalorieDataService _dataService;
        private readonly PhotoService _photoService;

        public EntryDetailViewModel(CalorieEntry entry, CalorieDataService dataService)
        {
            _entry = entry;
            _dataService = dataService;

            // Отримуємо PhotoService для видалення фото
            _photoService = App.Current.Handler.MauiContext.Services.GetService<PhotoService>();

            DeleteEntryCommand = new Command(async () => await DeleteEntry());
        }
        public CalorieEntry Entry => _entry;
        public ICommand DeleteEntryCommand { get; }

        /// <summary>
        /// Видалити запис
        /// </summary>
        private async Task DeleteEntry()
        {
            try
            {
                // Показуємо підтвердження
                var result = await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Підтвердження",
                    $"Ви впевнені, що хочете видалити запис:\n\n" +
                    $"«{Entry.Description}»\n" +
                    $"{Entry.Calories:F0} ккал\n\n" +
                    $"Цю дію неможливо скасувати!",
                    "🗑️ Видалити",
                    "❌ Скасувати");

                if (!result) return;

                // Видаляємо фото, якщо воно є
                if (!string.IsNullOrEmpty(Entry.PhotoPath))
                {
                    _photoService?.DeletePhoto(Entry.PhotoPath);
                }

                // Видаляємо запис з БД та колекції
                await _dataService.RemoveEntryAsync(Entry);

                // Показуємо повідомлення про успіх
                await Application.Current.MainPage.DisplayAlert(
                    "✅ Успіх!",
                    "Запис успішно видалено",
                    "OK");

                // Повертаємося на головну сторінку
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "❌ Помилка",
                    $"Не вдалося видалити запис:\n{ex.Message}",
                    "OK");
            }
        }


        // Допоміжний метод оновлення данних (загальний)
        // механізм для повідомлення UI про зміни даних у ViewModel.
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}