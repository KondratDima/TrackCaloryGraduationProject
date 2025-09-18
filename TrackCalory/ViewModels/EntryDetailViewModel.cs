using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using TrackCalory.Models;
using TrackCalory.Services;

namespace TrackCalory.ViewModels
{
    [QueryProperty(nameof(Entry), "Entry")]
    public class EntryDetailViewModel : INotifyPropertyChanged
    {
        private CalorieEntry _entry;
        private readonly DatabaseService _dataService;

        public event PropertyChangedEventHandler PropertyChanged;

        public CalorieEntry Entry
        {
            get => _entry;
            set
            {
                _entry = value;
                OnPropertyChanged();
            }
        }

        public ICommand DeleteEntryCommand { get; }

        public EntryDetailViewModel(DatabaseService dataService)
        {
            _dataService = dataService;
            DeleteEntryCommand = new Command(async () => await DeleteEntryAsync());
        }

        private async Task DeleteEntryAsync()
        {
            if (Entry == null) return;

            try
            {
                await _dataService.DeleteEntryAsync(Entry);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка видалення запису: {ex.Message}");
                await Shell.Current.DisplayAlert("Помилка", "Не вдалося видалити запис.", "OK");
            }
        }

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}