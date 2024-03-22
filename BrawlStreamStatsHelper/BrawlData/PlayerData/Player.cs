using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BrawlStreamStatsHelper.BrawlData.PlayerData.AttackData;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using BrawlStreamStatsHelper.CustomDataType;

namespace BrawlStreamStatsHelper.BrawlData.PlayerData
{
    public class Player
    {
        private string _playerNumber = string.Empty;
        private string _character = string.Empty;
        private string _playerName = string.Empty;
        private int _kOs;
        private int _teamKOs;
        private int _suicides;
        private int _teamNum = -1;
        private int _clashes;
        private int _dashes;
        private int _totalDodges;
        private int _airDodges;
        private int _dashJumps;
        private int _totalJumps;
        private int _airJumps;
        private DamageValue _damageDealt;
        private DamageValue _damageTaken;
        private DamageValue _teamDamageTaken;
        private DamageValue _teamDamageDealt;
        private int _score;
        private int _ballResets;
        private TimeValue _timeInAir;
        private TimeValue _timeOnGround;
        private TimeValue _timeOnWall;
        private double? _brawlballThrown;
        private TimeValue? _brawlballTimeHeld;
        private Dictionary<string, Weapon> _weapons;
        private List<SequenceData> _sequence;
        private WeaponData _totalWeaponData;
        private List<WeaponData> _weaponData;

        public string PlayerNumber
        {
            get => _playerNumber.Length <= 15 ? _playerNumber : _playerNumber.Substring(0, 15);
            set => _playerNumber = value;
        }


        public string Character
        {
            get => _character;
            set => _character = value;
        }        
        
        public string PlayerName
        {
            get
            {
                var asciiOnlyContent = Regex.Replace(_playerName, @"[^\x00-\x7F]", string.Empty);
                if (asciiOnlyContent != string.Empty && asciiOnlyContent.Length > 15)
                {
                    return asciiOnlyContent.Substring(0, 15) + "...";
                }
                else
                {
                    return asciiOnlyContent;
                }
            }
            set => _playerName = value;
        }

        public int KOs
        {
            get => _kOs;
            set => _kOs = value;
        }

        public int TeamKOs
        {
            get => _teamKOs;
            set => _teamKOs = value;
        }

        public int Suicides
        {
            get => _suicides;
            set => _suicides = value;
        }

        public int TeamNum
        {
            get => _teamNum;
            set => _teamNum = value;
        }

        public int Clashes
        {
            get => _clashes;
            set => _clashes = value;
        }

        public int Dashes
        {
            get => _dashes;
            set => _dashes = value;
        }

        public int TotalDodges
        {
            get => _totalDodges;
            set => _totalDodges = value;
        }

        public int AirDodges
        {
            get => _airDodges;
            set => _airDodges = value;
        }

        public int DashJumps
        {
            get => _dashJumps;
            set => _dashJumps = value;
        }

        public int TotalJumps
        {
            get => _totalJumps;
            set => _totalJumps = value;
        }

        public int AirJumps
        {
            get => _airJumps;
            set => _airJumps = value;
        }

        public DamageValue DamageDealt
        {
            get => _damageDealt;
            set => _damageDealt = value;
        }

        public DamageValue DamageTaken
        {
            get => _damageTaken;
            set => _damageTaken = value;
        }

        public DamageValue TeamDamageTaken
        {
            get => _teamDamageTaken;
            set => _teamDamageTaken = value;
        }

        public DamageValue TeamDamageDealt
        {
            get => _teamDamageDealt;
            set => _teamDamageDealt = value;
        }

        public int Score
        {
            get => _score;
            set => _score = value;
        }

        public int BallResets
        {
            get => _ballResets;
            set => _ballResets = value;
        }


        public TimeValue TimeInAir
        {
            get => _timeInAir / 1000;
            set => _timeInAir = value;
        }

        public TimeValue TimeOnGround
        {
            get => _timeOnGround / 1000;
            set => _timeOnGround = value;
        }

        public TimeValue TimeOnWall
        {
            get => _timeOnWall / 1000;
            set => _timeOnWall = value;
        }        
        public TimeValue? BrawlballTimeHeld
        {
            get => _brawlballTimeHeld;
            set => _brawlballTimeHeld = value;
        }

        public double? BrawlballThrown
        {
            get => _brawlballThrown;
            set => _brawlballThrown = value;
        }

        public Dictionary<string, Weapon> Weapons
        {
            get => _weapons;
            set => _weapons = value;
        }

        public List<SequenceData> Sequence
        {
            get => _sequence;
            set => _sequence = value;
        }
                
        public WeaponData TotalWeaponData
        {
            get => _totalWeaponData;
            set => _totalWeaponData = value;
        }       
        
        public List<WeaponData> WeaponData
        {
            get => _weaponData;
            set => _weaponData = value;
        }

        public Player()
        {
            _weapons = new Dictionary<string, Weapon>();
            _totalWeaponData = new WeaponData();
            _weaponData = new List<WeaponData>();
            _sequence = new List<SequenceData>();
        }
    }

}
