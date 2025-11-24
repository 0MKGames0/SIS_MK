using System.Collections.ObjectModel;
using System.Linq;
using SIS_MK.Models;
using SIS_MK.Services;

namespace SIS_MK.ViewModels
{
    public class DatabaseEditorViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        public ObservableCollection<CharacterDefinition> Characters { get; private set; }
        public ObservableCollection<ItemDefinition> Items { get; private set; }

        private string _statusText;

        public string StatusText
        {
            get { return _statusText; }
            set { SetField(ref _statusText, value); }
        }

        public DatabaseEditorViewModel(DataService dataService)
        {
            _dataService = dataService;

            AppData data = _dataService.LoadAppData();

            Characters = new ObservableCollection<CharacterDefinition>(data.Characters);
            Items = new ObservableCollection<ItemDefinition>(data.Items);

            StatusText = "Редактор базы. Не забывай нажимать «Сохранить».";
        }

        public void Save()
        {
            AppData data = new AppData
            {
                Characters = Characters.ToList(),
                Items = Items.ToList()
            };

            _dataService.SaveAppData(data);
            StatusText = "Сохранено.";
        }
    }
}
