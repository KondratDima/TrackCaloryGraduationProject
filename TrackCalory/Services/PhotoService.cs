using System;
using System.IO;
using System.Threading.Tasks;

namespace TrackCalory.Services
{
    public class PhotoService
    {
        private readonly string _photosDirectory;

        public PhotoService()
        {
            // Створюємо папку для фото у AppDataDirectory
            _photosDirectory = Path.Combine(FileSystem.AppDataDirectory, "FoodPhotos");

            if (!Directory.Exists(_photosDirectory))
            {
                Directory.CreateDirectory(_photosDirectory);
            }
        }

        /// <summary>
        /// Зробити фото через камеру
        /// </summary>
        public async Task<string> TakePhotoAsync()
        {
            try
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    throw new Exception("Камера не підтримується на цьому пристрої");
                }

                var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                {
                    Title = "Сфотографуйте страву"
                });

                return await SavePhotoAsync(photo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка фото з камери: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Вибрати фото з галереї
        /// </summary>
        public async Task<string> PickPhotoAsync()
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                {
                    Title = "Оберіть фото страви"
                });

                return await SavePhotoAsync(photo);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка вибору фото: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Зберегти фото у локальну папку
        /// </summary>
        private async Task<string> SavePhotoAsync(FileResult photo)
        {
            if (photo == null)
                return null;

            try
            {
                // Генеруємо унікальне ім'я файлу
                string fileName = $"food_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                string filePath = Path.Combine(_photosDirectory, fileName);

                // Копіюємо фото у нашу папку
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
                System.Diagnostics.Debug.WriteLine($"❌ Помилка збереження фото: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Видалити фото
        /// </summary>
        public void DeletePhoto(string photoPath)
        {
            try
            {
                if (!string.IsNullOrEmpty(photoPath) && File.Exists(photoPath))
                {
                    File.Delete(photoPath);
                    System.Diagnostics.Debug.WriteLine($"🗑️ Фото видалено: {photoPath}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка видалення фото: {ex.Message}");
            }
        }

        /// <summary>
        /// Очистити всі старі фото (наприклад, старіші за 30 днів)
        /// </summary>
        public void CleanupOldPhotos(int daysToKeep = 30)
        {
            try
            {
                var files = Directory.GetFiles(_photosDirectory, "*.jpg");
                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        System.Diagnostics.Debug.WriteLine($"🗑️ Видалено старе фото: {file}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка очищення старих фото: {ex.Message}");
            }
        }
    }
}