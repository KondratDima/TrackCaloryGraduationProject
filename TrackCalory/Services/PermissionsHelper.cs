using System;
using System.Threading.Tasks;

namespace TrackCalory.Services
{
    /// <summary>
    /// Допоміжний клас для роботи з дозволами на Android 13+
    /// Обробляє нову модель дозволів: "в цей раз", "при використанні додатку"
    /// </summary>
    public static class PermissionsHelper
    {
        /// <summary>
        /// Перевіряє та запитує дозвіл на камеру
        /// Працює з тимчасовими дозволами Android 13+
        /// </summary>
        public static async Task<bool> CheckAndRequestCameraPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();

                if (status == PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("Дозвіл на камеру вже наданий");
                    return true;
                }

                // Якщо дозвіл не наданий - запитуємо
                if (status == PermissionStatus.Denied || status == PermissionStatus.Unknown)
                {
                    // Показуємо пояснення перед запитом (рекомендовано Google)
                    if (Permissions.ShouldShowRationale<Permissions.Camera>())
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "📷 Потрібен доступ до камери",
                            "Додаток потребує доступ до камери для фотографування страв.\n\n" +
                            "Ви можете надати доступ:\n" +
                            "• Тільки один раз\n" +
                            "• При використанні додатку",
                            "Зрозуміло");
                    }

                    status = await Permissions.RequestAsync<Permissions.Camera>();
                }

                if (status == PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("✅ Користувач надав доступ до камери");
                    return true;
                }

                // Якщо користувач відмовив назавжди - пропонуємо перейти в налаштування
                if (status == PermissionStatus.Denied)
                {
                    bool goToSettings = await Application.Current.MainPage.DisplayAlert(
                        "❌ Доступ заборонено",
                        "Для використання камери потрібно надати дозвіл в налаштуваннях додатку.\n\n" +
                        "Перейти в налаштування?",
                        "Так",
                        "Ні");

                    if (goToSettings)
                    {
                        AppInfo.ShowSettingsUI();
                    }
                }

                System.Diagnostics.Debug.WriteLine($"⚠Дозвіл на камеру: {status}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка перевірки дозволу камери: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Перевіряє та запитує дозвіл на фото (READ_MEDIA_IMAGES для Android 13+)
        /// </summary>
        public static async Task<bool> CheckAndRequestPhotosPermissionAsync()
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Photos>();

                if (status == PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("Дозвіл на фото вже наданий");
                    return true;
                }

                // Показуємо пояснення
                if (status == PermissionStatus.Denied || status == PermissionStatus.Unknown)
                {
                    if (Permissions.ShouldShowRationale<Permissions.Photos>())
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "🖼️ Потрібен доступ до фото",
                            "Додаток потребує доступ до галереї для вибору фото страв.\n\n" +
                            "Ви можете надати доступ:\n" +
                            "• До обраних фото (Android 14+)\n" +
                            "• До всіх фото\n" +
                            "• Тільки один раз",
                            "Зрозуміло");
                    }

                    status = await Permissions.RequestAsync<Permissions.Photos>();
                }

                if (status == PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("Користувач надав доступ до фото");
                    return true;
                }

                if (status == PermissionStatus.Denied)
                {
                    bool goToSettings = await Application.Current.MainPage.DisplayAlert(
                        "❌ Доступ заборонено",
                        "Для вибору фото потрібно надати дозвіл в налаштуваннях додатку.\n\n" +
                        "Перейти в налаштування?",
                        "Так",
                        "Ні");

                    if (goToSettings)
                    {
                        AppInfo.ShowSettingsUI();
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Дозвіл на фото: {status}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка перевірки дозволу фото: {ex.Message}");
                return false;
            }
        }
    }
}