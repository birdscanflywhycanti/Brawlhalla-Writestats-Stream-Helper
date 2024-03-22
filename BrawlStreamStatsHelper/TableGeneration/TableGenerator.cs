using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BrawlStreamStatsHelper.BrawlData;
using BrawlStreamStatsHelper.BrawlData.PlayerData;
using BrawlStreamStatsHelper.BrawlData.PlayerData.AttackData;
using BrawlStreamStatsHelper.CustomDataType;
using Newtonsoft.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BrawlStreamStatsHelper.TableGeneration
{
    public class TableGenerator
    {
        private readonly GameData _gameData;
        private readonly Config _config;
        private readonly string _filePath;
        private string _tableFolder = string.Empty;

        public TableGenerator(GameData gameData, string filePath, Config config)
        {
            _gameData = gameData;
            _filePath = filePath;
            _config = config;



            if (_config == null)
            {
                throw new Exception("_config file could not be read!");
            }

        }

        public void Generate()
        {
            if (_config == null)
            {
                return;
            }

            _tableFolder = Path.Combine(_filePath, "Tables");
            if (Directory.Exists(_tableFolder))
            {
                Directory.Delete(_tableFolder, true);
            }

            Directory.CreateDirectory(_tableFolder);
            var teams = new Dictionary<string, List<Player>>();
            if (!_config.SortTablesByTeam)
            {
                teams.Add("No Team", new List<Player>());
            }
            foreach (var player in _gameData.Players)
            {
                if (_config.SortTablesByTeam)
                {
                    if (!teams.ContainsKey("Team " + player.TeamNum))
                    {
                        teams.Add("Team " + player.TeamNum, new List<Player>());
                    }
                    teams["Team " + player.TeamNum].Add(player);
                    continue;
                }
                teams["No Team"].Add(player);
            }



            foreach (var table in _config.Charts)
            {
                try
                {
                    Console.WriteLine("Generating '" + table.TableName + "'");
                    var chartData =
                        new Dictionary<string, List<Dictionary<string, string>>>();
                    foreach (var team in teams)
                    {
                        var teamData = new List<Dictionary<string, string>>();
                        foreach (var player in team.Value)
                        {
                            var playerData = new Dictionary<string, string>();
                            foreach (var stat in table.Fields)
                            {
                                playerData.Add(stat.Header, FetchStat(stat.Content, player));
                            }

                            teamData.Add(playerData);
                        }

                        if (table.SaveTeamsSeparately)
                        {
                            chartData.Add(team.Key, teamData);
                        }
                        else
                        {
                            if (!chartData.TryAdd("", teamData))
                            {
                                chartData[""].AddRange(teamData);
                            }
                        }

                    }

                    OutputTable(table, chartData);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("ERROR: " + table.TableName );
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Skipping Table...");
                }


            }
        }

        private void OutputTable(Table table, Dictionary<string, List<Dictionary<string, string>>> chartData)
        {
            foreach (var (teamName, list) in chartData)
            {
                var headers = list.FirstOrDefault()?.Keys.ToList() ?? new List<string>();
                var teamDataWithHeaders = new List<List<string>> { headers };

                foreach (var playerData in list)
                {
                    var rowData = headers.Select(header => playerData.TryGetValue(header, out var value) ? value : string.Empty).ToList();
                    teamDataWithHeaders.Add(rowData);
                }

                var data = ExpandData(teamDataWithHeaders);

                if (table.InvertHeaderOrientation)
                {
                    data = TransposeTeamData(data);
                }

                var outputText = BuildOutputText(data, table, false);
                if (table.EnableTxtExport)
                {
                    var fileName = $"{_filePath}/Tables/{table.TableName}{(table.SaveTeamsSeparately ? "_" + teamName : string.Empty)}.txt";
                    File.WriteAllText(fileName, outputText, Encoding.UTF8);
                }
                if (table.EnablePngExport)
                {
                    var fileName = $"{_filePath}/Tables/{table.TableName}{(table.SaveTeamsSeparately ? "_" + teamName : string.Empty)}.png";

                    CreateTransparentPngWithText(outputText, fileName, table);
                }

                if (!table.EnableCsvExport) continue;
                {
                    outputText = BuildOutputText(data, table, true);
                    var fileName = $"{_filePath}/Tables/{table.TableName}{(table.SaveTeamsSeparately ? "_" + teamName : string.Empty)}.csv";
                    File.WriteAllText(fileName, outputText, Encoding.UTF8);
                }
            }
        }

        private void CreateTransparentPngWithText(string text, string outputPath, Table table)
        {
            Font font;
            Font headerFont;
            var colorConverter = new ColorConverter();
            using (var fontTester = new Font(
                       _config.ImageSettings.FontName,
                       _config.ImageSettings.FontSize,
                       FontStyle.Bold))
            {
                font = new Font(FontFamily.GenericMonospace, _config.ImageSettings.FontSize, FontStyle.Regular);

                headerFont = new Font(FontFamily.GenericMonospace, _config.ImageSettings.FontSize, FontStyle.Bold);
            }

            using var tempBitmap = new Bitmap(1, 1);
            using var tempGraphics = Graphics.FromImage(tempBitmap);
            tempGraphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            var lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);


            float extraPadding = 10;

            float maxWidth = 0;
            float totalHeight = 10; // Start with initial padding
            List<(string Line, SizeF Size, bool NeedsPadding)> lineDetails = new List<(string, SizeF, bool)>();

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var size = tempGraphics.MeasureString(line, headerFont);
                var needsPadding = !line.StartsWith(" ") && i > 0; // Check if padding is needed above this line

                lineDetails.Add((line, size, needsPadding));

                maxWidth = Math.Max(maxWidth, size.Width);
                totalHeight += size.Height + (needsPadding ? extraPadding : 0); // Add extra padding if needed
            }

            var width = (int)Math.Ceiling(maxWidth) + 20;
            var height = (int)Math.Ceiling(totalHeight) + 10; // Add padding at the bottom

            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.Transparent);
            if (_config.ImageSettings.AddBackground)
                graphics.Clear((Color)(colorConverter.ConvertFromString(_config.ImageSettings.BackgroundColour) ?? Color.White));

            graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            float currentHeight = 10; // Start with initial padding
            var row1 = false; // Flag to keep track of the row color toggle
            var rowColor = (Color)(colorConverter.ConvertFromString(_config.ImageSettings.HeaderColour) ?? Color.Black);
            var currFont = headerFont;
            var isLastLineHeader = true;

            foreach (var (line, size, needsPadding) in lineDetails)
            {

                if (needsPadding)
                {
                    row1 = !row1; // Toggle the row color flag
                    if (row1)
                    {
                        rowColor = (Color)(colorConverter.ConvertFromString(_config.ImageSettings.RowColour1) ?? Color.Black);
                    }
                    else
                    {
                        rowColor = (Color)(colorConverter.ConvertFromString(_config.ImageSettings.RowColour2) ?? Color.Black);
                    }
                    currentHeight += extraPadding; // Add extra padding above this line
                }

                using var textBrush = new SolidBrush(rowColor);
                // Draw the line with the selected brush
                var textPosition = new PointF(0, currentHeight);


                graphics.DrawString(line, currFont, textBrush, textPosition);
                currentHeight += size.Height; // Move to the position for the next line
                if (!isLastLineHeader) continue;
                if (line[0] != ' ') continue;
                currFont = font;
                isLastLineHeader = false;

            }

            // Save the bitmap with the drawn text
            bitmap.Save(outputPath, ImageFormat.Png);

        }



        private static List<List<string>> ExpandData(List<List<string>> data)
        {
            var expandedData = new List<List<string>>();
            foreach (var row in data)
            {
                var tempRows = new List<List<string>>();
                var maxSplits = row.Select(cell => cell.Split(new string[] { "//" }, StringSplitOptions.None)).Select(splits => splits.Length).Prepend(0).Max();


                // Initialize temporary rows to hold the split data
                for (var i = 0; i < maxSplits; i++)
                {
                    tempRows.Add(new List<string>(new string[row.Count])); // Create a new row with the same number of columns as the original
                }

                // Distribute the split data across the temporary rows
                for (var columnIndex = 0; columnIndex < row.Count; columnIndex++)
                {
                    var cellSplits = row[columnIndex].Split(new string[] { "//" }, StringSplitOptions.None);
                    for (var splitIndex = 0; splitIndex < maxSplits; splitIndex++)
                    {
                        if (splitIndex < cellSplits.Length)
                        {
                            tempRows[splitIndex][columnIndex] = cellSplits[splitIndex];
                        }
                        else
                        {
                            tempRows[splitIndex][columnIndex] = ""; // Fill with empty string if there are no more splits
                        }
                    }
                }

                // Add a prefix for subsequent rows to indicate continuation
                foreach (var tempRow in tempRows)
                {
                    if (tempRow != tempRows.First()) // Skip the first row
                    {
                        tempRow[0] = ""; // Assuming the first column is some sort of identifier, like a player name
                    }
                    expandedData.Add(tempRow);
                }
            }

            return expandedData;
        }


        private static List<List<string>> TransposeTeamData(List<List<string>> teamData)
        {
            var transposedData = new List<List<string>>();
            if (teamData.Count == 0) return transposedData;

            var columnCount = teamData.First().Count;
            for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                var columnData = teamData.Select(row => row[columnIndex]).ToList();
                transposedData.Add(columnData);
            }

            return transposedData;
        }


        private static string BuildOutputText(List<List<string>> data, Table table, bool isCsv)
        {
            var outputText = new StringBuilder();

            // Calculate the maximum length of each column if not in CSV mode
            List<int> columnWidths = new();
            if (!isCsv)
            {
                columnWidths = new List<int>();
                for (var columnIndex = 0; columnIndex < data[0].Count; columnIndex++)
                {
                    var maxLength = data.Max(row => row[columnIndex].Length);
                    columnWidths.Add(maxLength);
                }
            }

            foreach (var row in data)
            {
                for (var columnIndex = 0; columnIndex < row.Count; columnIndex++)
                {
                    var cell = row[columnIndex];
                    if (!isCsv)
                    {
                        // Pad the cell to align columns
                        cell = cell.PadRight(columnWidths[columnIndex]);
                    }
                    outputText.Append(cell);

                    if (columnIndex < row.Count - 1)
                    {
                        outputText.Append(isCsv ? ", " : "   ");
                    }
                }
                outputText.AppendLine();
            }

            return outputText.ToString();
        }



        private static string FetchStat(string statName, Player player)
        {
            foreach (var property in typeof(Player).GetProperties())
            {
                if (!CheckProperty(statName, property)) continue;
                var value = property.GetValue(player)?.ToString() ?? "";
                return value;
            }



            if (statName.StartsWith("Sep"))
            {
                statName = statName[$"Sep".Length..];
                var outputData = "";
                foreach (var weapon in player.WeaponData)
                {
                    if (outputData != "")
                    {
                        outputData += "//";
                    }
                    foreach (var property in typeof(WeaponData).GetProperties())
                    {
                        if (!CheckProperty(statName, property)) continue;
                        var value = property.GetValue(weapon)?.ToString() ?? "";
                        outputData += value;
                        break;
                    }
                }

                return outputData;

            }

            foreach (var property in typeof(WeaponData).GetProperties())
            {
                if (!CheckProperty(statName, property)) continue;
                var value = property.GetValue(player.TotalWeaponData)?.ToString() ?? "";
                return value;
            }


            throw new Exception("Unable to find stat " + statName);
        }

        private static bool CheckProperty(string statName, PropertyInfo property)
        {
            if (property.PropertyType != typeof(int) &&
                property.PropertyType != typeof(double) &&
                property.PropertyType != typeof(string) &&
                property.PropertyType != typeof(TimeValue) &&
                property.PropertyType != typeof(DamageValue) &&
                Nullable.GetUnderlyingType(property.PropertyType) != typeof(int) &&
                Nullable.GetUnderlyingType(property.PropertyType) != typeof(double)&&
                Nullable.GetUnderlyingType(property.PropertyType) != typeof(TimeValue) &&
                Nullable.GetUnderlyingType(property.PropertyType) != typeof(DamageValue))
                return false;
            return property.Name == statName;
        }
    }
}
