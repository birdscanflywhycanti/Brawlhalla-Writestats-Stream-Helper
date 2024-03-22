using BrawlStreamStatsHelper.BrawlData;
using BrawlStreamStatsHelper.BrawlData.PlayerData;
using BrawlStreamStatsHelper.BrawlData.PlayerData.AttackData;
using Newtonsoft.Json.Linq;
using System.Globalization;
using BrawlStreamStatsHelper.TableGeneration;
using Object = System.Object;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;
using BrawlStreamStatsHelper.CustomDataType;

namespace BrawlStreamStatsHelper
{
    public class BrawlStreamStatsHelper
    {
        private static string _userProfilePath = "";
        private readonly FileSystemWatcher _watcher;
        private static Config _config = new();

        /// <summary>
        /// Gets the folder path of the stat dump folder
        /// </summary>
        /// <param name="brawlhallaStatDumpsPath"></param>
        /// <returns></returns>
        private static void InitFolderPath(out string brawlhallaStatDumpsPath)
        {
            Console.WriteLine("Locating BrawlhallaStatDumps");
            _userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            brawlhallaStatDumpsPath = Path.Combine(_userProfilePath, "BrawlhallaStatDumps");

            if (Directory.Exists(brawlhallaStatDumpsPath))
            {
                Console.WriteLine("Directory Found:");
                Console.WriteLine(brawlhallaStatDumpsPath);
                return;
            }

            Console.WriteLine("Directory does not exist at " + brawlhallaStatDumpsPath);
            Console.WriteLine("Creating Directory...");
            Directory.CreateDirectory(brawlhallaStatDumpsPath);
        }

        /// <summary>
        /// Sets up class and hooks events for the watcher
        /// </summary>
        /// <exception cref="Exception"></exception>
        public BrawlStreamStatsHelper()
        {
            InitFolderPath(out var folderPath);


            _watcher = new FileSystemWatcher
            {
                Path = folderPath,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
            };
            _watcher.Path = folderPath;
            Console.WriteLine("Files will be stored at:");
            Console.WriteLine(Path.Combine(_userProfilePath, "BrawlStreamGameData/"));
            _watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;

            _watcher.Created += OnChanged;
            _watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Fires when a new file is added
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Get the path of the directory
            var directory = new DirectoryInfo(((FileSystemWatcher)source).Path);


            var newestFile = directory.GetFiles().MaxBy(f => f.LastWriteTime);

            if (newestFile == null) return;
            Console.WriteLine("New File Detected:");
            Console.WriteLine(newestFile.FullName);

            var gameData = ReadFile(newestFile.FullName);

            var configFilePath = Path.Combine(_userProfilePath, "BrawlStreamGameData");
            configFilePath = Path.Combine(configFilePath, "Config.json");


            if (File.Exists(configFilePath))
            {
                // Read the configuration file content
                var configContent = File.ReadAllText(configFilePath, Encoding.UTF8);

                // Try to deserialize the configuration content
                var tmp = JsonConvert.DeserializeObject<Config>(configContent);

                // If deserialization returns null (invalid content), create a new Config object
                if (tmp == null)
                {
                    WriteConfigToFile(configFilePath); // Write the new, default configuration back to the file
                }
                else
                {
                    _config = tmp;
                }


            }
            else
            {

                WriteConfigToFile(configFilePath); // Write the new, default configuration to a new file
            }

            TimeValue.DecimalPlaces = _config.TimeRoundingDp;
            DamageValue.DecimalPlaces = _config.DamageRoundingDp;
            PercentageValue.DecimalPlaces = _config.PercentageRoundingDp;
            if (gameData != null)
            {
                WriteFiles(gameData);
            }
        }


        private static void WriteConfigToFile(string filePath)
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                if (directoryPath != null) Directory.CreateDirectory(directoryPath);
            }

            // Serialize the Config object and write it to the specified file
            _config = new Config
            {
                Charts = new List<Table>
                {
                    new Table()
                    {
                        TableName = "Example Table1",
                        Fields = new List<Field>
                        {
                            new Field("Player", "PlayerName"),
                            new Field("Team", "TeamNum"),
                            new Field("Total Damage//Dealt", "DamageDealt"),
                            new Field("Overall//Accuracy", "Accuracy"),
                            new Field("Weapon//Name", "SepWeaponName"),
                            new Field("Damage//Dealt", "SepDamageDealt"),
                            new Field("Accuracy", "SepAccuracy")
                        }
                    },
                    new Table()
                    {
                        TableName = "Example Table2",
                        SaveTeamsSeparately = false,
                        Fields = new List<Field>
                        {
                            new Field("Player", "PlayerName"),
                            new Field("Team", "TeamNum"),
                            new Field("Total Damage//Dealt", "DamageDealt"),
                            new Field("Overall//Accuracy", "Accuracy"),
                            new Field("Weapon//Name", "SepWeaponName"),
                            new Field("Damage//Dealt", "SepDamageDealt"),
                            new Field("Accuracy", "SepAccuracy")
                        }
                    }
                }
            };
            var configJson = JsonConvert.SerializeObject(_config, Formatting.Indented);
            File.WriteAllText(filePath, configJson, Encoding.UTF8);
        }


        /// <summary>
        /// Parses the json file into a class containing the game data
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static GameData? ReadFile(string filePath)
        {
            JObject? jsonObject = null;
            const int maxRetries = 5;
            var attempts = 0;

            while (attempts < maxRetries)
            {
                // Attempt to deserialize the file
                try
                {
                    var content = File.ReadAllText(filePath, Encoding.UTF8);
                    jsonObject = JObject.Parse(content);
                    break; // Success, exit loop
                }
                catch (IOException)
                {
                    attempts++;
                    Thread.Sleep(500); // Wait for 500ms before retrying
                }
            }

            if (jsonObject == null)
            {
                Console.WriteLine("Failed to read file after multiple attempts.");
                return null;
            }
            var gameData = jsonObject.ToObject<GameData>();

            if (gameData != null)
            {
                // Populate the data structure for any number of players and unknown weapons
                gameData.Players = jsonObject.Properties()
                    .Where(p => p.Name.StartsWith("Player"))
                    .Select(p =>
                    {
                        // Populate player class
                        var player = p.Value.ToObject<Player>();
                        player!.PlayerNumber = p.Name;
                        player.Character = p.Value["Loadout"]!["LegendName"]?.ToString()!;
                        var weapons = p.Value.Children<JProperty>()
                            .Where(prop => prop.Value.Type == JTokenType.Object && prop.Value["TimeHeld"] != null)
                            .ToDictionary(
                                prop => prop.Name,
                                prop =>
                                {
                                    var weapon = prop.Value.ToObject<Weapon>();
                                    weapon!.Name = prop.Name;
                                    return weapon;
                                });

                        player.Weapons = weapons;

                        return player;
                    })
                    .ToList();


            }
            return gameData;
        }

        /// <summary>
        /// Handles writing game info to folders
        /// </summary>
        /// <param name="gameData"></param>
        private static void WriteFiles(GameData gameData)
        {
            try
            {
                // Attempt to write files for game data
                try
                {
                    var rootPath = Path.Combine(_userProfilePath, "BrawlStreamGameData/GameData");
                    if (Directory.Exists(rootPath))
                    {
                        Directory.Delete(rootPath, true);
                    }

                    Directory.CreateDirectory(rootPath);
                    WriteGameLevelInfo(gameData, rootPath);
                    Console.WriteLine("Updated Data");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to update Game Data:");
                    throw new Exception(ex.Message);

                }

                // Attempt to draw and save tables
                try
                {
                    var tableGenerator = new TableGenerator(gameData, _userProfilePath + "/BrawlStreamGameData/", _config);
                    tableGenerator.Generate();
                    Console.WriteLine("Finished Generating Tables!");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }

        }

        /// <summary>
        /// Handles writing info to the folders at a game level
        /// </summary>
        /// <param name="gameData"></param>
        /// <param name="rootPath"></param>
        private static void WriteGameLevelInfo(GameData gameData, string rootPath)
        {

            // Custom variables for brawlball and team level stats
            double brawlballTotalTimeHeld = 0;
            var teamBallTimeHeld = new double[2];
            var teamAttacksUsed = new double[2];
            var teamAttacksHit = new double[2];

            if (_config.SortFilesByTeam)
            {
                // Split teams out
                Dictionary<string, List<Player>> playersByTeam = new();
                Dictionary<string, int> playerCounter = new();
                foreach (var player in gameData.Players)
                {
                    if (!playersByTeam.ContainsKey("Team" + player.TeamNum))
                    {
                        playersByTeam.Add("Team" + player.TeamNum, new List<Player>());
                        playerCounter.Add("Team" + player.TeamNum, 0);
                    }
                    playersByTeam["Team" + player.TeamNum].Add(player);
                    playerCounter["Team" + player.TeamNum]++;
                    player.PlayerNumber = "Player" + playerCounter["Team" + player.TeamNum];
                }

                // Calculate and populate player and weapon level stats for each team
                foreach (var team in playersByTeam)
                {
                    foreach (var player in team.Value)
                    {
                        var playerDirectory = Path.Combine(rootPath, team.Key);
                        playerDirectory = Path.Combine(playerDirectory, player.PlayerNumber);
                        Directory.CreateDirectory(playerDirectory);

                        WritePlayerLevelGameInfo(player, playerDirectory);

                        WriteWeaponLevelInfo(playerDirectory, player, ref brawlballTotalTimeHeld, ref teamBallTimeHeld, ref teamAttacksHit, ref teamAttacksUsed);
                        WriteClass(player, playerDirectory);
                    }
                }
            }
            else
            {
                // Calculate and populate player and weapon level stats for each player
                foreach (var player in gameData.Players)
                {
                    var playerDirectory = Path.Combine(rootPath, player.PlayerNumber);
                    Directory.CreateDirectory(playerDirectory);

                    WritePlayerLevelGameInfo(player, playerDirectory);

                    WriteWeaponLevelInfo(playerDirectory, player, ref brawlballTotalTimeHeld, ref teamBallTimeHeld, ref teamAttacksHit, ref teamAttacksUsed);
                    WriteClass(player, playerDirectory);
                }
            }


            // Populate and save game level stats
            gameData.BallTotalTimeHeld = brawlballTotalTimeHeld;
            gameData.Team1BallPossession = CalculateAndFormatPercentage(teamBallTimeHeld[0], brawlballTotalTimeHeld);
            gameData.Team2BallPossession = CalculateAndFormatPercentage(teamBallTimeHeld[1], brawlballTotalTimeHeld);
            gameData.Team1Accuracy = CalculateAndFormatPercentage(teamAttacksHit[0], teamAttacksUsed[0]);
            gameData.Team2Accuracy = CalculateAndFormatPercentage(teamAttacksHit[1], teamAttacksUsed[1]);
            if (gameData is { Team1BallPossession: "N/A", Team2BallPossession: "N/A" })
            {
                gameData.Team1BallPossession = "";
                gameData.Team2BallPossession = "";
            }
            WriteClass(gameData, rootPath);
        }

        /// <summary>
        /// Handles Writing General Weapon level info
        /// </summary>
        /// <param name="playerDirectory"></param>
        /// <param name="player"></param>
        /// <param name="brawlballTotalTimeHeld"></param>
        /// <param name="teamBallTimeHeld"></param>
        /// <param name="teamAttacksHit"></param>
        /// <param name="teamAttacksUsed"></param>
        private static void WriteWeaponLevelInfo(string playerDirectory, Player player, ref double brawlballTotalTimeHeld,
            ref double[] teamBallTimeHeld,
            ref double[] teamAttacksHit,
            ref double[] teamAttacksUsed)
        {
            var weaponDirectory = Path.Combine(playerDirectory, "WeaponStats");
            Directory.CreateDirectory(weaponDirectory);
            var weaponDataList = new List<WeaponData>();
            var weaponIndex = 0;
            foreach (var key in player.Weapons.Keys)
            {
                var weaponText = "Wep" + weaponIndex;
                switch (key)
                {
                    case "BrawlballNoHit":
                        brawlballTotalTimeHeld += player.Weapons[key].TimeHeld;
                        teamBallTimeHeld[player.TeamNum - 1] += player.Weapons[key].TimeHeld;

                        player.BrawlballThrown = player.Weapons[key].Throw.Uses;
                        player.BrawlballTimeHeld = player.Weapons[key].TimeHeld;
                        continue;
                    case "Unarmed":
                        weaponText = "Unarmed";
                        break;
                    default:
                        weaponIndex++;
                        break;
                }

                var weaponSubDirectory = Path.Combine(weaponDirectory, weaponText);
                Directory.CreateDirectory(weaponSubDirectory);
                var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "/Assets/Weapons/", key.ToLower() + ".png");
                File.Copy(sourcePath, weaponSubDirectory + "/Weapon.png", overwrite: true);
                var weaponData = GetWeaponData(player, key,
                    ref teamAttacksUsed,
                    ref teamAttacksHit);
                WriteClass(weaponData, weaponSubDirectory);
                weaponDataList.Add(weaponData);

            }
            // Populate weapon data and save to file
            var totalWeaponData = new WeaponData
            {
                WeaponName = string.Empty,
                TimeHeld = 0,
                TimesThrown = 0,
                DamageDealt = 0,
                Accuracy = string.Empty,
                HeavyDamageDealt = 0,
                HeavyUses = 0,
                HeavyHits = 0,
                HeavyAccuracy = string.Empty,
                LightDamageDealt = 0,
                LightUses = 0,
                LightHits = 0,
                LightAccuracy = string.Empty
            };

           
            foreach (var weapon in weaponDataList)
            {
                totalWeaponData.HeavyDamageDealt += weapon.HeavyDamageDealt;
                totalWeaponData.LightDamageDealt += weapon.LightDamageDealt;
                totalWeaponData.DamageDealt += weapon.DamageDealt;
                totalWeaponData.HeavyUses += weapon.HeavyUses;
                totalWeaponData.HeavyHits += weapon.HeavyHits;
                totalWeaponData.LightHits += weapon.LightHits;
                totalWeaponData.LightUses += weapon.LightUses;
                totalWeaponData.TimeHeld += weapon.TimeHeld;
                totalWeaponData.TimesThrown += weapon.TimesThrown;
            }

            totalWeaponData.Accuracy = CalculateAndFormatPercentage(totalWeaponData.LightHits + totalWeaponData.HeavyHits, totalWeaponData.LightUses + totalWeaponData.HeavyUses);
            totalWeaponData.LightAccuracy = CalculateAndFormatPercentage(totalWeaponData.LightHits, totalWeaponData.LightUses);
            totalWeaponData.HeavyAccuracy = CalculateAndFormatPercentage(totalWeaponData.HeavyHits, totalWeaponData.HeavyUses);

            WriteClass(totalWeaponData, weaponDirectory);
            player.TotalWeaponData = totalWeaponData;
            player.WeaponData = weaponDataList;
        }


        /// <summary>
        /// Handles calculating game info at a weapon specific level
        /// </summary>
        /// <param name="player"></param>
        /// <param name="key"></param>
        /// <param name="teamAttacksUsed"></param>
        /// <param name="teamAttacksHit"></param>
        /// <returns></returns>
        private static WeaponData GetWeaponData(Player player, string key,
            ref double[] teamAttacksUsed,
            ref double[] teamAttacksHit)
        {
            var data = new WeaponData
            {
                WeaponName = player.Weapons[key].Name,
                TimesThrown = player.Weapons[key].Throw.Uses,
                TimeHeld = player.Weapons[key].TimeHeld
            };

            foreach (var property in player.Weapons[key].GetType().GetProperties())
            {
                if (property.PropertyType.IsSubclassOf(typeof(Attack)) || property.PropertyType == typeof(Attack))
                {
                    var attack = (Attack)property.GetValue(player.Weapons[key])!;
                    data.DamageDealt += attack.EnemyDamage;
                    if (player.TeamNum != -1)
                    {
                        teamAttacksUsed[player.TeamNum - 1] += attack.Uses;
                        teamAttacksHit[player.TeamNum - 1] += attack.EnemyHits;
                    }
                }

                if (property.PropertyType == typeof(HeavyAttack))
                {
                    data.HeavyDamageDealt += (((Attack)property.GetValue(player.Weapons[key])!)).EnemyDamage;
                    data.HeavyUses += (((Attack)property.GetValue(player.Weapons[key])!)).Uses;
                    data.HeavyHits += ((Attack)property.GetValue(player.Weapons[key])!).EnemyHits;
                }
                else if (property.PropertyType == typeof(LightAttack) || property.PropertyType == typeof(AirAttack))
                {
                    data.LightDamageDealt += ((Attack)property.GetValue(player.Weapons[key])!).EnemyDamage;
                    data.LightUses += ((Attack)property.GetValue(player.Weapons[key])!).Uses;
                    data.LightHits += ((Attack)property.GetValue(player.Weapons[key])!).EnemyHits;
                }
            }

            data.Accuracy = CalculateAndFormatPercentage(data.LightHits + data.HeavyHits, data.LightUses + data.HeavyUses);
            data.LightAccuracy = CalculateAndFormatPercentage(data.LightHits, data.LightUses);
            data.HeavyAccuracy = CalculateAndFormatPercentage(data.HeavyHits, data.HeavyUses);

            return data;
        }

        /// <summary>
        /// Calculates and formats percentage splits between two values
        /// </summary>
        /// <param name="paramA"></param>
        /// <param name="paramB"></param>
        /// <returns></returns>
        private static string CalculateAndFormatPercentage(double paramA, double paramB)
        {
            if (paramB == 0) return "N/A";

            PercentageValue accuracy = (paramA / paramB) * 100f;
            return accuracy + "%";
        }

        /// <summary>
        /// Adds the avatar icon to the player folder and the legend name
        /// </summary>
        /// <param name="player"></param>
        /// <param name="playerDirectory"></param>
        private static void WritePlayerLevelGameInfo(Player player, string playerDirectory)
        {
            var legendName = player.Character.ToLower().Replace(" ", "_");
            var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory + "/Assets/Legends/", legendName + ".png");
            File.Copy(sourcePath, playerDirectory + "/Legend.png", overwrite: true);

        }

        /// <summary>
        /// Writes the class to a folder
        /// </summary>
        /// <param name="data"></param>
        /// <param name="weaponSubDirectory"></param>
        private static void WriteClass(object data, string weaponSubDirectory)
        {
            foreach (var property in data.GetType().GetProperties())
            {
                // Filter out properties based on their type early.
                if (property.PropertyType != typeof(int) &&
                    property.PropertyType != typeof(double) &&
                    property.PropertyType != typeof(string) &&
                    property.PropertyType != typeof(TimeValue) &&
                    property.PropertyType != typeof(DamageValue) &&
                    Nullable.GetUnderlyingType(property.PropertyType) != typeof(int) &&
                    Nullable.GetUnderlyingType(property.PropertyType) != typeof(double) &&
                    Nullable.GetUnderlyingType(property.PropertyType) != typeof(TimeValue) &&
                    Nullable.GetUnderlyingType(property.PropertyType) != typeof(DamageValue))
                    continue;

                // Get the value of the property. If it's null or an empty string, continue.
                var value = property.GetValue(data)?.ToString() ?? "";
                if (string.IsNullOrEmpty(value))
                    continue;



                // Write the value to a file named after the property.
                var filePath = Path.Combine(weaponSubDirectory, property.Name + ".txt");
                File.WriteAllText(filePath, value, Encoding.UTF8);
            }
        }

    }
}

