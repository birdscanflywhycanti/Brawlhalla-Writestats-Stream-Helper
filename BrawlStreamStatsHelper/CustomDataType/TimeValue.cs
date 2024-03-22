namespace BrawlStreamStatsHelper.CustomDataType
{
    public struct PercentageValue : IEquatable<PercentageValue>, IComparable<PercentageValue>
    {
        public static int DecimalPlaces = 1;
        private double value;

        private PercentageValue(double value)
        {
            this.value = Math.Round(value, DecimalPlaces);
        }

        public static implicit operator double(PercentageValue PercentageValue)
        {
            return Math.Round(PercentageValue.value, DecimalPlaces);
        }

        public static implicit operator PercentageValue(double value)
        {
            return new PercentageValue(value);
        }

        public bool Equals(PercentageValue other)
        {
            return Math.Abs(Math.Round(value, DecimalPlaces) - Math.Round(other.value, DecimalPlaces)) < 0.05;
        }

        public int CompareTo(PercentageValue other)
        {
            return Math.Round(value, DecimalPlaces).CompareTo(Math.Round(other.value, DecimalPlaces));
        }

        public override bool Equals(object? obj) => obj is PercentageValue other && Equals(other);

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

