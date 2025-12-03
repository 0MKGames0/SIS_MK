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
        private readonly string _progressPath;
        private readonly string _gamesPath;

        // Для каждой игры путь к items-файлу зависит от выбранной игры
        private string _itemsPath;

        private readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };

        private AppData _appData = new AppData();
        private ProfileProgress _profile = new ProfileProgress();

        private readonly List<GameDefinition> _games = new();
        private GameDefinition _currentGame;

        public IReadOnlyList<GameDefinition> Games => _games;
        public GameDefinition CurrentGame => _currentGame;

        public DataService()
        {
            _dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(_dataFolder);

            _gamesPath = Path.Combine(_dataFolder, "games.json");
            _progressPath = Path.Combine(_dataFolder, "progress.json");

            LoadGames();
        }

        private void LoadGames()
        {
            _games.Clear();

            if (File.Exists(_gamesPath))
            {
                string json = File.ReadAllText(_gamesPath);
                var loaded = JsonSerializer.Deserialize<List<GameDefinition>>(json, _jsonOptions)
                             ?? new List<GameDefinition>();

                foreach (var game in loaded)
                {
                    if (game != null && !string.IsNullOrWhiteSpace(game.Id))
                    {
                        if (string.IsNullOrWhiteSpace(game.ItemsFile))
                        {
                            game.ItemsFile = "items.json";
                        }

                        _games.Add(game);
                    }
                }
            }

            // Если файл отсутствует или пуст — создаём дефолтную игру на базе твоего текущего items.json
            if (_games.Count == 0)
            {
                var defaultGame = new GameDefinition
                {
                    Id = "outbreak_file1",
                    Name = "Resident Evil Outbreak: File #1",
                    ItemsFile = "items.json"
                };

                _games.Add(defaultGame);
                SaveGames();
            }

            _currentGame = _games[0];

            if (string.IsNullOrWhiteSpace(_currentGame.ItemsFile))
            {
                _currentGame.ItemsFile = "items.json";
            }

            _itemsPath = Path.Combine(_dataFolder, _currentGame.ItemsFile);
        }

        private void SaveGames()
        {
            string json = JsonSerializer.Serialize(_games, _jsonOptions);
            File.WriteAllText(_gamesPath, json);
        }

        public void SetCurrentGame(GameDefinition game)
        {
            if (game == null)
                throw new ArgumentNullException(nameof(game));

            if (_currentGame != null && _currentGame.Id == game.Id)
                return;

            _currentGame = game;

            if (string.IsNullOrWhiteSpace(_currentGame.ItemsFile))
            {
                _currentGame.ItemsFile = "items.json";
            }

            _itemsPath = Path.Combine(_dataFolder, _currentGame.ItemsFile);
        }

        private string CurrentGameId => _currentGame?.Id ?? string.Empty;

        public AppData LoadAppData()
        {
            if (!File.Exists(_itemsPath))
            {
                // Для базового items.json создаём демо-данные,
                // для новых игр — пустая база, чтобы ты сам её заполнял в редакторе.
                if (string.Equals(Path.GetFileName(_itemsPath), "items.json", StringComparison.OrdinalIgnoreCase))
                {
                    _appData = CreateSampleData();
                }
                else
                {
                    _appData = new AppData();
                }

                SaveAppData();
            }
            else
            {
                string json = File.ReadAllText(_itemsPath);
                _appData = JsonSerializer.Deserialize<AppData>(json, _jsonOptions) ?? new AppData();

                if (_appData.Items == null)
                    _appData.Items = new List<ItemDefinition>();

                if (_appData.Characters == null)
                    _appData.Characters = new List<CharacterDefinition>();
            }

            LoadProgress();
            return _appData;
        }

        public void SaveAppData(AppData data = null)
        {
            if (data != null)
            {
                _appData = data;

                if (_appData.Items == null)
                    _appData.Items = new List<ItemDefinition>();

                if (_appData.Characters == null)
                    _appData.Characters = new List<CharacterDefinition>();
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
                _profile = JsonSerializer.Deserialize<ProfileProgress>(json, _jsonOptions)
                           ?? new ProfileProgress();

                if (_profile.Items == null)
                    _profile.Items = new List<ItemProgress>();
            }

            NormalizeProgressGameIds();
        }

        /// <summary>
        /// Старые записи прогресса не знают про GameId.
        /// Тут мы всем пустым GameId прописываем id первой игры (твой текущий Outbreak File#1).
        /// </summary>
        private void NormalizeProgressGameIds()
        {
            if (_profile.Items == null)
                _profile.Items = new List<ItemProgress>();

            string defaultGameId = _games.Count > 0 ? _games[0].Id : "outbreak_file1";
            bool changed = false;

            foreach (var entry in _profile.Items)
            {
                if (string.IsNullOrWhiteSpace(entry.GameId))
                {
                    entry.GameId = defaultGameId;
                    changed = true;
                }
            }

            if (changed)
            {
                SaveProgress();
            }
        }

        private void SaveProgress()
        {
            string json = JsonSerializer.Serialize(_profile, _jsonOptions);
            File.WriteAllText(_progressPath, json);
        }

        public bool GetItemCollected(string characterId, string itemId)
        {
            string gameId = CurrentGameId;

            return _profile.Items.Any(p =>
                p.GameId == gameId &&
                p.CharacterId == characterId &&
                p.ItemId == itemId &&
                p.IsCollected);
        }

        public void SetItemCollected(string characterId, string itemId, bool isCollected)
        {
            string gameId = CurrentGameId;

            ItemProgress entry = _profile.Items
                .FirstOrDefault(p =>
                    p.GameId == gameId &&
                    p.CharacterId == characterId &&
                    p.ItemId == itemId);

            if (entry == null)
            {
                entry = new ItemProgress
                {
                    GameId = gameId,
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
                        Id = "basic_dagger",
                        Name = "Обычный кинжал",
                        Location = "Где-то в коридоре",
                        Scenario = "Outbreak",
                        Room = "Коридор",
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
