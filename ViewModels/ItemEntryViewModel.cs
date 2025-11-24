using SIS_MK.Models;
using SIS_MK.Services;

namespace SIS_MK.ViewModels
{
    public class ItemEntryViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        public ItemDefinition Definition { get; }
        public string CharacterId { get; }

        private bool _isCollected;

        public ItemEntryViewModel(ItemDefinition definition, string characterId, bool isCollected, DataService dataService)
        {
            Definition = definition;
            CharacterId = characterId;
            _isCollected = isCollected;
            _dataService = dataService;
        }

        public string Name => Definition.Name;
        public string Location => Definition.Location;

        public string Tooltip
        {
            get
            {
                var scenario = Definition.Scenario?.Trim();
                var room = Definition.Room?.Trim();

                if (string.IsNullOrWhiteSpace(scenario) && string.IsNullOrWhiteSpace(room))
                    return string.Empty;

                if (string.IsNullOrWhiteSpace(scenario))
                    return room ?? string.Empty;

                if (string.IsNullOrWhiteSpace(room))
                    return $"Сценарий: {scenario}";

                // основной вариант
                return $"Сценарий: {scenario} {room}";
            }
        }


        public bool IsCollected
        {
            get => _isCollected;
            set
            {
                if (SetField(ref _isCollected, value))
                {
                    _dataService.SetItemCollected(CharacterId, Definition.Id, _isCollected);
                }
            }
        }
    }
}
