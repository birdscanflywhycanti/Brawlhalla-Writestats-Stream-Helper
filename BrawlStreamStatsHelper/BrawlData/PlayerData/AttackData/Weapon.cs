using BrawlStreamStatsHelper.CustomDataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlStreamStatsHelper.BrawlData.PlayerData.AttackData
{
    public class Weapon
    {
        public string Name { get; set; } = string.Empty;
        private TimeValue _timeHeld;
        public TimeValue TimeHeld
        {
            get => _timeHeld / 1000;
            set => _timeHeld = value;
        }
        public HeavyAttack nHeavy { get; set; } = new();
        public HeavyAttack sHeavy { get; set; } = new();
        public HeavyAttack dHeavy { get; set; } = new();
        public LightAttack nLight { get; set; } = new();
        public LightAttack sLight { get; set; } = new();
        public LightAttack dLight { get; set; } = new();
        public AirAttack nAir { get; set; } = new();
        public AirAttack sAir { get; set; } = new();
        public AirAttack dAir { get; set; } = new();
        public LightAttack Recovery { get; set; } = new();
        public LightAttack Throw { get; set; } = new();

    }
}
