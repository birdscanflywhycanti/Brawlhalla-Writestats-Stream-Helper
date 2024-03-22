using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlStreamStatsHelper.TableGeneration
{
    public class ImageSettings
    {
        public string DevComment { get; set; } =
            "When choosing a font, ensure you use a monospaced font type so that columns and rows stay aligned. The font must be installed on your system to work.";
        public int FontSize { get; set; } = 12;
        public FontFamily FontName { get; set; } = new(GenericFontFamilies.Monospace);
        public bool AddBackground { get; set; } = false;
        public string HeaderColour { get; set; } = "#eb4c34";
        public string RowColour1 { get; set; } = "#303030";
        public string RowColour2 { get; set; } = "#171717";
        public string BackgroundColour { get; set; } = "#ffffff";

    }
}
