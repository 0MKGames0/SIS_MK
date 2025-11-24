using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using SIS_MK.Models;

namespace SIS_MK.Services
{
    public class DataService
    {
        private readonly string _dataFolder;
        private readonly string _itemsPath;
        private readonly string _progressPath;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

        private AppData _appData = new AppData();
        private ProfileProgress _profile = new ProfileProgress();

        public DataService()
        {
            _dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(_dataFolder);

            _itemsPath = Path.Combine(_dataFolder, "items.json");
            _progressPath = Path.Combine(_dataFolder, "progress.json");
        }

        public AppData LoadAppData()
        {
            if (!File.Exists(_itemsPath))
            {
                _appData = CreateSampleData();
                SaveAppData();
            }
            else
            {
                string json = File.ReadAllText(_itemsPath);
                _appData = JsonSerializer.Deserialize<AppData>(json, _jsonOptions) ?? new AppData();
            }

            LoadProgress();
            return _appData;
        }

        public void SaveAppData(AppData data = null)
        {
            if (data != null)
            {
                _appData = data;
            }

            string json = JsonSerializer.Serialize(_appData, _jsonOptions);
            File.WriteAllText(_itemsPath, json);
        }

        private void LoadProgress()
        {
            if (!File.Exists(_progressPath))
            {
                _profile = new ProfileProgress { Name = "Default" };
                SaveProgress();
            }
            else
            {
                string json = File.ReadAllText(_progressPath);
                _profile = JsonSerializer.Deserialize<ProfileProgress>(json, _jsonOptions) ?? new ProfileProgress();
            }
        }

        private void SaveProgress()
        {
            string json = JsonSerializer.Serialize(_profile, _jsonOptions);
            File.WriteAllText(_progressPath, json);
        }

        public bool GetItemCollected(string characterId, string itemId)
        {
            return _profile.Items.Any(p => p.CharacterId == characterId && p.ItemId == itemId && p.IsCollected);
        }

        public void SetItemCollected(string characterId, string itemId, bool isCollected)
        {
            ItemProgress entry = _profile.Items.FirstOrDefault(p => p.CharacterId == characterId && p.ItemId == itemId);
            if (entry == null)
            {
                entry = new ItemProgress
                {
                    CharacterId = characterId,
                    ItemId = itemId,
                    IsCollected = isCollected
                };
                _profile.Items.Add(entry);
            }
            else
            {
                entry.IsCollected = isCollected;
            }

            SaveProgress();
        }

        private AppData CreateSampleData()
        {
            return new AppData
            {
                Characters = new List<CharacterDefinition>
                {
                    new CharacterDefinition
                    {
                        Id = "david",
                        Name = "Дэвид",
                        Portrait = "Images/Characters/david.png"
                    },
                    new CharacterDefinition
                    {
                        Id = "shared",
                        Name = "Общие предметы",
                        Portrait = "Images/Characters/shared.png"
                    }
                },
                Items = new List<ItemDefinition>
                {
                    new ItemDefinition
                    {
                        Id = "boots_room30",
                        Name = "Ботинки",
                        Location = "Примерно в комнате #30",
                        Scenario = "Outbreak",
                        Room = "Комната #30",
                        AvailableFor = new List<string> { "david", "shared" }
                    },
                    new ItemDefinition
                    {
                        Id = "golden_nail_puller",
                        Name = "Золотой гвоздодёр",
                        Location = "В подсобке, путь D",
                        Scenario = "Outbreak",
                        Room = "Подсобка",
                        AvailableFor = new List<string> { "david", "shared" }
                    }
                }
            };
        }

        public IReadOnlyList<CharacterDefinition> Characters
        {
            get { return _appData.Characters; }
        }

        public IReadOnlyList<ItemDefinition> Items
        {
            get { return _appData.Items; }
        }
    }
}
