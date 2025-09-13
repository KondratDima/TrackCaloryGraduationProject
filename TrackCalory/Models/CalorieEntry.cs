using System;
using System.ComponentModel;

namespace TrackCalory.Models
{
    // Це МОДЕЛЬ - описує, як виглядає один запис про калорії
    public class CalorieEntry : INotifyPropertyChanged
    {
        // ПОЛЯ (властивості) нашої моделі:

        private DateTime _date;
        private double _calories;
        private string _description;

        // ID запису (унікальний номер)
        public int Id { get; set; }

        // Дата, коли з'їли їжу
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
        public double Calories
        {
            get => _calories;
            set
            {
                _calories = value;
                OnPropertyChanged(); // Повідомляємо інтерфейс про зміну
            }
        }

        // Опис страви (наприклад: "Омлет з овочами")
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(); // Повідомляємо інтерфейс про зміну
            }
        }

        // Це потрібно для того, щоб інтерфейс автоматично оновлювався
        // коли змінюються дані
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

