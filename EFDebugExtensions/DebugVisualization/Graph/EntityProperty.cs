using System.Diagnostics;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    [DebuggerDisplay("{Description}")]
    public class EntityProperty
    {
        public string Name { get; set; }
        public object OriginalValue { get; set; }
        public object CurrentValue { get; set; }

        public bool IsKey { get; set; }

        public string Description
        {
            get { return Name + ": " + TrimToMaxLength(CurrentValue) + (HasChanged ? " (changed from " + TrimToMaxLength(OriginalValue) + ")" : ""); }
        }

        public bool HasChanged
        {
            get
            {
                if (CurrentValue == null && OriginalValue == null)
                    return false;

                return CurrentValue != null && !CurrentValue.Equals(OriginalValue);
            }
        }

        private static string TrimToMaxLength(object value)
        {
            if (value == null)
                return null;

            const int maxLength = 150;
            var toTrim = value.ToString();
            if (toTrim.Length <= maxLength)
                return toTrim;

            return toTrim.Substring(0, maxLength) + " [..]";
        }
    }
}