using System;
using System.ComponentModel;
using SQLite;

namespace TrackCalory.Models
{
    // Це МОДЕЛЬ ТАБЛИЦЯ - описує, як виглядає один запис про калорії
    [Table("CalorieEntries")]
    public class CalorieEntry : INotifyPropertyChanged
    {
        // ПОЛЯ властивості нашої моделі:

        private DateTime _date;
        private double _calories;
        private string _description;

        // ID запису 
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Дата, коли зїли їжу
        [NotNull]
        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(); // Повідомляємо інтерфейс про зміну
            }
        }

        // Кількість калорій
        [NotNull]
        public double Calories
        {
            get => _calories;
            set
            {
                _calories = value;
                OnPropertyChanged(); 
            }
        }

        // Опис страви (наприклад: "Омлет з овочами")
        [MaxLength(500)]
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(); 
            }
        }

        [MaxLength(100)] // катигорія та БЖУ
        public string Category { get; set; } = "Основна страва";

        public double? Protein { get; set; }
        public double? Fat { get; set; }
        public double? Carbs { get; set; }

        [Indexed] // індекс для швидшого пошуку
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Це потрібно для того, щоб інтерфейс автоматично оновлювався
        // коли змінюються дані
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

