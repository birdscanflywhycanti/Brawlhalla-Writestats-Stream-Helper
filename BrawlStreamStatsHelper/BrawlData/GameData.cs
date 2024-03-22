using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using BrawlStreamStatsHelper.BrawlData.PlayerData;
using BrawlStreamStatsHelper.CustomDataType;

namespace BrawlStreamStatsHelper.BrawlData
{
    public class GameData
    {
        public string GameMode { get; set; } = string.Empty;
        public string MapName { get; set; } = string.Empty;
        public string Team1BallPossession { get; set; } = string.Empty;
        public string Team2BallPossession { get; set; } = string.Empty;        
        public string Team1Accuracy { get; set; } = string.Empty;
        public string Team2Accuracy { get; set; } = string.Empty;
        public TimeValue? BallTotalTimeHeld { get; set; }


        private double _gameDuration;

        public double GameDuration
        {
            get => _gameDuration / 1000;
            set => _gameDuration = value;
        }
        public int ScoreToWin { get; set; } = 0;
        public List<Player> Players { get; set; } = new();

        public GameData()
        {
        }
    }
}
