using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlStreamStatsHelper.TableGeneration
{
    public class Table
    {

        public string TableName { get; init; } = "Example Table";
        public bool EnablePngExport { get; set; } = true;
        public bool EnableTxtExport { get; set; } = true;
        public bool EnableCsvExport { get; set; } = true;
        public bool SaveTeamsSeparately { get; init; } = true;
        public bool InvertHeaderOrientation { get; set; } = false;

        public List<Field> Fields { get; init; } = new();

    }
}
