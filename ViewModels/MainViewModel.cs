using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SIS_MK.Models;
using SIS_MK.Services;

namespace SIS_MK.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly DataService _dataService;

        private readonly ObservableCollection<ItemEntryViewModel> _itemsInternal =
            new ObservableCollection<ItemEntryViewModel>();

        private readonly ObservableCollection<string> _scenarios =
            new ObservableCollection<string>();

        private CharacterDefinition _selectedCharacter;
        private string _selectedScenario = "Все сценарии";

        private int _collectedCount;
        private int _totalCount;
        private string _counterText = "00 / 00 собрано";

        public MainViewModel()
        {
            _dataService = new DataService();
            var data = _dataService.LoadAppData();

            Characters = new ObservableCollection<CharacterDefinition>(data.Characters);
            Items = _itemsInternal;
            Scenarios = _scenarios;

            if (Characters.Count > 0)
            {
                // по умолчанию — первый персонаж, фильтр "Все сценарии"
                _selectedScenario = "Все сценарии";
                _selectedCharacter = Characters[0];
                OnPropertyChanged(nameof(SelectedCharacter));
                OnPropertyChanged(nameof(SelectedScenario));

                ReloadItemsAndScenarios(true);
            }
        }

        public ObservableCollection<CharacterDefinition> Characters { get; }

        public ObservableCollection<ItemEntryViewModel> Items { get; }

        public ObservableCollection<string> Scenarios { get; }

        public CharacterDefinition SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                if (SetField(ref _selectedCharacter, value))
                {
                    // сменился персонаж — пересчитываем и список предметов, и список сценариев
                    ReloadItemsAndScenarios(true);
                }
            }
        }

        public string SelectedScenario
        {
            get => _selectedScenario;
            set
            {
                if (SetField(ref _selectedScenario, value))
                {
                    // сменился фильтр — пересчитываем только список предметов
                    ReloadItemsAndScenarios(false);
                }
            }
        }

        public int CollectedCount
        {
            get => _collectedCount;
            private set => SetField(ref _collectedCount, value);
        }

        public int TotalCount
        {
            get => _totalCount;
            private set => SetField(ref _totalCount, value);
        }

        public string CounterText
        {
            get => _counterText;
            private set => SetField(ref _counterText, value);
        }

        /// <summary>
        /// rebuildScenarios = true  -> пересчитать список сценариев.
        /// rebuildScenarios = false -> только переложить предметы,
        ///                             сценарии остаются как есть.
        /// </summary>
        private void ReloadItemsAndScenarios(bool rebuildScenarios)
        {
            _itemsInternal.Clear();

            if (_selectedCharacter == null)
            {
                _scenarios.Clear();
                UpdateCounters();
                return;
            }

            // 1. Берём все предметы ТОЛЬКО для выбранного персонажа
            var allItemsForCharacter = _dataService.Items
                .Where(i => i.AvailableFor != null &&
                            i.AvailableFor.Contains(_selectedCharacter.Id))
                .OrderBy(i => i.Name)
                .ToList();

            // 2. Обновляем список сценариев (если нужно)
            if (rebuildScenarios)
            {
                _scenarios.Clear();
                _scenarios.Add("Все сценарии");

                var scenarios = allItemsForCharacter
                    .Select(i => (i.Scenario ?? string.Empty).Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .OrderBy(s => s);

                foreach (var s in scenarios)
                    _scenarios.Add(s);

                if (string.IsNullOrWhiteSpace(_selectedScenario) ||
                    !_scenarios.Contains(_selectedScenario))
                {
                    _selectedScenario = "Все сценарии";
                    OnPropertyChanged(nameof(SelectedScenario));
                }
            }

            // 3. Применяем фильтр по сценарию
            var filtered = allItemsForCharacter.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_selectedScenario) &&
                _selectedScenario != "Все сценарии")
            {
                filtered = filtered.Where(i =>
                    string.Equals(
                        (i.Scenario ?? string.Empty).Trim(),
                        _selectedScenario,
                        StringComparison.OrdinalIgnoreCase));
            }

            // 4. Собираем VM для каждого предмета
            foreach (var def in filtered)
            {
                bool isCollected = _dataService.GetItemCollected(_selectedCharacter.Id, def.Id);
                var vm = new ItemEntryViewModel(def, _selectedCharacter.Id, isCollected, _dataService);
                vm.PropertyChanged += Item_PropertyChanged;
                _itemsInternal.Add(vm);
            }

            UpdateCounters();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemEntryViewModel.IsCollected))
            {
                UpdateCounters();
            }
        }

        private void UpdateCounters()
        {
            TotalCount = Items.Count;
            CollectedCount = Items.Count(i => i.IsCollected);
            CounterText = $"{CollectedCount:00} / {TotalCount:00} собрано";
        }

        public void ReloadDataFromDisk()
        {
            var data = _dataService.LoadAppData();

            Characters.Clear();
            foreach (var ch in data.Characters)
                Characters.Add(ch);

            if (Characters.Count > 0)
            {
                SelectedCharacter = Characters[0]; // автоматически перезагрузит сценарии и предметы
            }
            else
            {
                _selectedCharacter = null;
                OnPropertyChanged(nameof(SelectedCharacter));

                _itemsInternal.Clear();
                _scenarios.Clear();
                UpdateCounters();
            }
        }
    }
}
