using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlStreamStatsHelper.TableGeneration
{
    public class Field
    {
        public string Header { get; set; }
        public string Content { get; set; }

        public Field(string header, string content)
        {
            Header = header;
            Content = content;
        }
    }
}
