using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrackCalory.Services
{
    /// <summary>
    /// Сервіс для розпізнавання їжі на фото через Google Gemini AI.
    /// 
    /// ЛОГІКА РОБОТИ:
    /// 1. Отримує шлях до фото (зберігається через PhotoService)
    /// 2. Читає фото як байти
    /// 3. Конвертує в Base64 (формат для API)
    /// 4. Відправляє до Gemini API з промптом
    /// 5. Парсить JSON відповідь з калоріями та БЖУ
    /// 
    /// ВЗАЄМОДІЯ З PhotoService:
    /// - PhotoService зберігає фото → повертає шлях
    /// - GeminiVisionService читає фото за шляхом → аналізує
    /// 
    /// МОДЕЛЬ: gemini-2.0-flash 
    /// ТЕМПЕРАТУРА: 0.1 
    /// </summary>
    public class GeminiVisionService
    {
        private readonly HttpClient _httpClient;
        private readonly ConfigurationService _configService;
        private readonly string _apiKey;
        private readonly double _temperature;

        private const string API_BASE_URL = "https://generativelanguage.googleapis.com/v1beta/models";

        public GeminiVisionService(ConfigurationService configService)
        {
            _configService = configService;
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Таймаут 30 сек

            // Завантажуємо налаштування з appsettings.json
            _apiKey = _configService.GetGeminiApiKey();
            _temperature = _configService.GetTemperature();

            System.Diagnostics.Debug.WriteLine($"✅ GeminiVisionService ініціалізовано (temp={_temperature})");
        }

        /// <summary>
        /// Розпізнає їжу на фото за шляхом до файлу
        /// </summary>
        /// <param name="photoPath">Шлях до фото (з PhotoService)</param>
        /// <returns>Результат аналізу з калоріями, БЖУ, назвою</returns>
        public async Task<FoodAnalysisResult> AnalyzeFoodFromPathAsync(string photoPath)
        {
            try
            {
                // КРОК 1: Перевіряємо чи існує файл
                if (string.IsNullOrEmpty(photoPath) || !File.Exists(photoPath))
                {
                    return new FoodAnalysisResult { Error = "Файл фото не знайдено" };
                }

                // КРОК 2: Читаємо фото як масив байтів
                byte[] imageBytes = await File.ReadAllBytesAsync(photoPath);

                System.Diagnostics.Debug.WriteLine($"📸 Розмір фото: {imageBytes.Length / 1024} KB");

                // КРОК 3: Відправляємо на аналіз
                return await AnalyzeFoodImageAsync(imageBytes);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка читання фото: {ex.Message}");
                return new FoodAnalysisResult { Error = $"Помилка читання файлу: {ex.Message}" };
            }
        }

        /// <summary>
        /// Основний метод аналізу зображення
        /// </summary>
        private async Task<FoodAnalysisResult> AnalyzeFoodImageAsync(byte[] imageBytes)
        {
            try
            {
                // КРОК 1: Конвертуємо фото в Base64
                // Base64 - це текстове представлення бінарних даних
                // Gemini API приймає зображення ТІЛЬКИ у такому форматі
                string base64Image = Convert.ToBase64String(imageBytes);

                System.Diagnostics.Debug.WriteLine($"📤 Відправляємо {base64Image.Length} символів Base64...");

                // КРОК 2: Формуємо тіло запиту
                // Структура Gemini API: contents → parts → [text, inline_data]
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                // Текстова частина - промпт (інструкція для AI)
                                new
                                {
                                    text = GetAnalysisPrompt()
                                },
                                // Зображення у Base64
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = "image/jpeg",
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    },
                    // Налаштування генерації
                    generationConfig = new
                    {
                        temperature = _temperature,  // 0.1 - точні відповіді
                        topK = 32,
                        topP = 1,
                        maxOutputTokens = _configService.GetMaxTokens()
                    }
                };

                // КРОК 3: Серіалізуємо в JSON
                string jsonRequest = JsonConvert.SerializeObject(requestBody);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                // КРОК 4: Формуємо URL з API ключем
                string modelName = _configService.GetModelName();
                string url = $"{API_BASE_URL}/{modelName}:generateContent?key={_apiKey}";

                // КРОК 5: Відправляємо POST запит
                var response = await _httpClient.PostAsync(url, content);

                // КРОК 6: Обробка помилок
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ API Error {response.StatusCode}: {errorContent}");

                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        return new FoodAnalysisResult { Error = "⏳ Перевищено ліміт запитів. Спробуйте за хвилину." };
                    }

                    return new FoodAnalysisResult { Error = $"Помилка API: {response.StatusCode}" };
                }

                // КРОК 7: Парсимо відповідь
                string responseJson = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"📥 Отримано відповідь від Gemini");

                // Структура відповіді: candidates[0].content.parts[0].text
                var geminiResponse = JObject.Parse(responseJson);
                string textResponse = geminiResponse["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString();

                if (string.IsNullOrEmpty(textResponse))
                {
                    return new FoodAnalysisResult { Error = "Порожня відповідь від AI" };
                }

                // КРОК 8: Очищаємо від markdown (```json ... ```)
                textResponse = CleanJsonResponse(textResponse);

                System.Diagnostics.Debug.WriteLine($"🤖 AI відповідь: {textResponse.Substring(0, Math.Min(150, textResponse.Length))}...");

                // КРОК 9: Парсимо JSON результат
                var result = JsonConvert.DeserializeObject<FoodAnalysisResult>(textResponse);

                if (result == null)
                {
                    return new FoodAnalysisResult { Error = "Не вдалося розпарсити відповідь" };
                }

                System.Diagnostics.Debug.WriteLine($"✅ Розпізнано: {result.DishName}, {result.Calories} ккал");
                return result;
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка мережі: {ex.Message}");
                return new FoodAnalysisResult { Error = "Помилка підключення до інтернету" };
            }
            catch (JsonException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Помилка JSON: {ex.Message}");
                return new FoodAnalysisResult { Error = "AI повернув некоректний формат даних" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Загальна помилка: {ex.Message}");
                return new FoodAnalysisResult { Error = $"Помилка: {ex.Message}" };
            }
        }

        /// <summary>
        /// Промпт для AI - інструкція як аналізувати фото
        /// Температура 0.1 робить відповіді максимально точними
        /// </summary>
        private string GetAnalysisPrompt()
        {
            return @"Проаналізуй це зображення їжі та поверни точну інформацію про калорійність.

ВАЖЛИВО: Поверни ТІЛЬКИ JSON, без додаткового тексту, без markdown форматування.

Формат відповіді:
{
  ""dishName"": ""назва страви українською"",
  ""calories"": число_калорій_на_всю_порцію,
  ""protein"": грами_білка,
  ""fat"": грами_жиру,
  ""carbs"": грами_вуглеводів,
  ""weight"": приблизна_вага_порції_у_грамах,
  ""confidence"": рівень_впевненості_від_0_до_1
}

Якщо на фото немає їжі або неможливо розпізнати:
{
  ""error"": ""На фото не виявлено їжі""
}

ПРАВИЛА:
1. Калорії вказуй для ВСІЄЇ порції на фото
2. Назву пиши українською мовою
3. Будь максимально точним у розрахунках
4. Враховуй український контекст страв (борщ, вареники, сало тощо)
5. Якщо сумніваєшся - вказуй confidence < 0.7";
        }

        /// <summary>
        /// Очищає відповідь від markdown форматування
        /// Gemini іноді обгортає JSON у ```json ... ```
        /// </summary>
        private string CleanJsonResponse(string response)
        {
            response = response.Trim();

            if (response.StartsWith("```json"))
                response = response.Substring(7);
            else if (response.StartsWith("```"))
                response = response.Substring(3);

            if (response.EndsWith("```"))
                response = response.Substring(0, response.Length - 3);

            return response.Trim();
        }
    }

    /// <summary>
    /// Модель результату аналізу їжі
    /// Відповідає JSON структурі, яку повертає Gemini
    /// </summary>
    public class FoodAnalysisResult
    {
        [JsonProperty("dishName")]
        public string DishName { get; set; }

        [JsonProperty("calories")]
        public double Calories { get; set; }

        [JsonProperty("protein")]
        public double? Protein { get; set; }

        [JsonProperty("fat")]
        public double? Fat { get; set; }

        [JsonProperty("carbs")]
        public double? Carbs { get; set; }

        [JsonProperty("weight")]
        public double? Weight { get; set; }

        [JsonProperty("confidence")]
        public double? Confidence { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        /// <summary>
        /// Перевірка чи результат валідний
        /// </summary>
        [JsonIgnore]
        public bool IsValid => string.IsNullOrEmpty(Error) && Calories > 0;

        /// <summary>
        /// Форматована інформація для показу користувачу
        /// </summary>
        [JsonIgnore]
        public string FormattedSummary
        {
            get
            {
                if (!IsValid) return Error ?? "Невідома помилка";

                var summary = $"🍽️ {DishName}\n";
                summary += $"🔥 {Calories:F0} ккал";

                if (Weight.HasValue)
                    summary += $" (~{Weight:F0} г)";

                if (Protein.HasValue || Fat.HasValue || Carbs.HasValue)
                {
                    summary += $"\n📊 Б: {Protein:F1}г | Ж: {Fat:F1}г | В: {Carbs:F1}г";
                }

                if (Confidence.HasValue)
                    summary += $"\n🎯 Впевненість: {Confidence:P0}";

                return summary;
            }
        }
    }
}