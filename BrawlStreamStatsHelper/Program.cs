using BrawlStreamStatsHelper.BrawlData;
using BrawlStreamStatsHelper.BrawlData.PlayerData;
using BrawlStreamStatsHelper.BrawlData.PlayerData.AttackData;
using BrawlStreamStatsHelper.TableGeneration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BrawlStreamStatsHelper
{
    internal class Program
    {

        static void Main(string[] args)
        {

            var brawlStreamStatsHelper = new BrawlStreamStatsHelper();

            Console.WriteLine("Monitoring folder. Press 'q' to quit.");
            Console.WriteLine();
            while (Console.Read() != 'q') ;
        }
    }
}