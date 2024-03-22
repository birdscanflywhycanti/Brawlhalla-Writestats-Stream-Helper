using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrawlStreamStatsHelper.CustomDataType
{
    public struct DamageValue : IEquatable<DamageValue>, IComparable<DamageValue>
    {
        public static int DecimalPlaces = 1;
        private double value;

        private DamageValue(double value)
        {
            this.value = Math.Round(value, DecimalPlaces);
        }

        public static implicit operator double(DamageValue DamageValue)
        {
            return Math.Round(DamageValue.value, DecimalPlaces);
        }

        public static implicit operator DamageValue(double value)
        {
            return new DamageValue(value);
        }

        public bool Equals(DamageValue other)
        {
            return Math.Abs(Math.Round(value, DecimalPlaces) - Math.Round(other.value, DecimalPlaces)) < 0.05;
        }

        public int CompareTo(DamageValue other)
        {
            // Compare the rounded values
            return Math.Round(value, DecimalPlaces).CompareTo(Math.Round(other.value, DecimalPlaces));
        }

        public override bool Equals(object? obj) => obj is DamageValue other && Equals(other);

        public override int GetHashCode()
        {
            return Math.Round(value, DecimalPlaces).GetHashCode();
        }
        public override string ToString()
        {
            // Format the value to have the specified number of decimal places
            return value.ToString($"F{DecimalPlaces}");
        }
    }
}

