using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlStreamStatsHelper.BrawlData.PlayerData.AttackData
{
    public abstract class Attack
    {
        public int EnemyKOs { get; set; } = 0;
        public int Uses { get; set; } = 0;
        public int EnemyHits { get; set; } = 0;
        public int EnemyDamage { get; set; } = 0;

    }
}
