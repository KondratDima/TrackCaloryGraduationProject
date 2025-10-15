﻿using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace TrackCalory.Services
{
    /// <summary>
    /// Сервіс для читання налаштувань з appsettings.json
    /// 
    /// ЛОГІКА:
    /// 1. Читає вбудований appsettings.json (EmbeddedResource)
    /// 2. Парсить JSON та витягує налаштування
    /// 3. Надає доступ до API ключа та інших параметрів
    /// 
    /// ЧОМУ ТАК:
    /// - Файл вбудовується в додаток при компіляції
    /// - Не потрібен доступ до файлової системи
    /// </summary>
    public class ConfigurationService
    {
        private readonly JObject _configuration;

        public ConfigurationService()
        {
            // Завантажуємо appsettings.json з вбудованих ресурсів
            _configuration = LoadConfiguration();
        }

        /// <summary>
        /// Читає appsettings.json з EmbeddedResource
        /// </summary>
        private JObject LoadConfiguration()
        {
            try
            {
                // Отримуємо поточну збірку (Assembly)
                var assembly = Assembly.GetExecutingAssembly();

                // Повна назва ресурсу: TrackCalory.appsettings.json
                string resourceName = "TrackCalory.appsettings.json";

                // Відкриваємо потік для читання
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        throw new FileNotFoundException($"Файл конфігурації не знайдено: {resourceName}");
                    }

                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        return JObject.Parse(json);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка завантаження конфігурації: {ex.Message}");
                throw new Exception("Не вдалося завантажити appsettings.json. Перевірте чи файл додано як EmbeddedResource.");
            }
        }

        /// <summary>
        /// Отримує API ключ Gemini
        /// </summary>
        public string GetGeminiApiKey()
        {
            try
            {
                string apiKey = _configuration["GeminiApiSettings"]?["ApiKey"]?.ToString();

                if (string.IsNullOrWhiteSpace(apiKey) )
                {
                    throw new InvalidOperationException(
                        "API ключ не налаштований в appsettings.json. " );
                }

                return apiKey;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка отримання API ключа: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Отримує назву моделі (gemini-1.5-flash)
        /// </summary>
        public string GetModelName()
        {
            return _configuration["GeminiApiSettings"]?["ModelName"]?.ToString() ?? "gemini-1.5-flash";
        }

        /// <summary>
        /// Отримує температуру генерації (0.1 для точності)
        /// </summary>
        public double GetTemperature()
        {
            string tempStr = _configuration["GeminiApiSettings"]?["Temperature"]?.ToString();
            return double.TryParse(tempStr, out double temp) ? temp : 0.1;
        }

        /// <summary>
        /// Отримує максимум токенів у відповіді
        /// </summary>
        public int GetMaxTokens()
        {
            string maxStr = _configuration["GeminiApiSettings"]?["MaxTokens"]?.ToString();
            return int.TryParse(maxStr, out int max) ? max : 1024;
        }

        /// <summary>
        /// Перевіряє чи налаштовано API ключ
        /// </summary>
        public bool IsApiKeyConfigured()
        {
            try
            {
                string apiKey = _configuration["GeminiApiSettings"]?["ApiKey"]?.ToString();
                return !string.IsNullOrWhiteSpace(apiKey) &&
                       apiKey != "YOUR_GEMINI_API_KEY_HERE" &&
                       apiKey.StartsWith("AIza");
            }
            catch
            {
                return false;
            }
        }
    }
}