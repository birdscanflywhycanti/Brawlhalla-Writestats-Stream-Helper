using BrawlStreamStatsHelper.CustomDataType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlStreamStatsHelper.BrawlData.PlayerData.AttackData
{
    public class WeaponData
    {
        public string WeaponName { get; set; } = string.Empty;
        public TimeValue TimeHeld { get; set; } = 0;
        public TimeValue TimesThrown { get; set; } = 0;
        public DamageValue DamageDealt { get; set; } = 0;
        public string Accuracy { get; set; } = string.Empty;
        public DamageValue HeavyDamageDealt { get; set; } = 0;
        public int HeavyUses { get; set; } = 0;
        public int HeavyHits { get; set; } = 0;
        public string HeavyAccuracy { get; set; } = string.Empty;
        public DamageValue LightDamageDealt { get; set; } = 0;
        public int LightUses { get; set; } = 0;
        public int LightHits { get; set; } = 0;
        public string LightAccuracy { get; set; } = string.Empty;
    }
}
