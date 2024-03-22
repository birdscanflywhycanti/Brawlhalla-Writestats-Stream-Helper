using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrawlStreamStatsHelper.TableGeneration;

namespace BrawlStreamStatsHelper
{
    public class Config
    {
        public string DevComment { get; set; } = "Delete this file to regenerate the default template";
        public bool SortTablesByTeam { get; set; } = true;
        public bool SortFilesByTeam { get; set; } = true;
        public ImageSettings ImageSettings { get; set; } = new();

        public int TimeRoundingDp = 1;
        public int PercentageRoundingDp = 0;
        public int DamageRoundingDp = 2;

        public string DevComment1 { get; set; } = "{'ColumnName', 'Field'}. You may use '//' in the title to indicate a new line.";
        public string DevComment2 { get; set; } = "Reference any fields in GameData by name. It is Case Sensitive and must match exactly.";
        public string DevComment3 { get; set; } = "When adding a Weapon stat, the stat name on its own will refer to the total value. Adding 'Sep' to the start will split it into a weapon specific stat";
        public List<Table> Charts { get; set; } = new();

    }
}
