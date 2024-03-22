namespace BrawlStreamStatsHelper.CustomDataType
{
    public struct TimeValue : IEquatable<TimeValue>, IComparable<TimeValue>
    {
        public static int DecimalPlaces = 1;
        private double value;

        private TimeValue(double value)
        {
            // Round the value when it's set to ensure internal consistency
            this.value = Math.Round(value, DecimalPlaces);
        }

        public static implicit operator double(TimeValue timeValue)
        {
            // Round the value when it's retrieved
            return Math.Round(timeValue.value, DecimalPlaces);
        }

        public static implicit operator TimeValue(double value)
        {
            return new TimeValue(value);
        }

        // Implement additional functionality here

        public bool Equals(TimeValue other)
        {
            // Use the rounded values for comparison to ensure consistency
            return Math.Abs(Math.Round(value, DecimalPlaces) - Math.Round(other.value, DecimalPlaces)) < 0.05;
        }

        public int CompareTo(TimeValue other)
        {
            // Compare the rounded values
            return Math.Round(value, DecimalPlaces).CompareTo(Math.Round(other.value, DecimalPlaces));
        }

        // It's a good practice to override Equals(object obj) when implementing IEquatable<T>
        public override bool Equals(object? obj) => obj is TimeValue other && Equals(other);

        // Override GetHashCode as well when Equals is overridden
        public override int GetHashCode()
        {
            // Use the rounded value for hash code generation to ensure consistency
            return Math.Round(value, DecimalPlaces).GetHashCode();
        }

        public override string ToString()
        {
            // Format the value to have the specified number of decimal places
            return value.ToString($"F{DecimalPlaces}");
        }
    }
}
