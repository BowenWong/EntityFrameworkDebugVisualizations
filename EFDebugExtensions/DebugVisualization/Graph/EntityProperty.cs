using System;
using System.Data.Entity;
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
        public bool IsConcurrencyProperty { get; set; }
        public bool IsRelation { get; set; }

        public EntityState EntityState { get; set; }

        public string Description
        {
            get { return GetDescription(); }
        }

        private string GetDescription()
        {
            switch (EntityState)
            {
                case EntityState.Added:
                    return IsKey || IsConcurrencyProperty ? Name + ": <not assigned yet>" : Name + ": " + TrimToMaxLength(CurrentValue);
                case EntityState.Unchanged:
                    return Name + ": " + TrimToMaxLength(CurrentValue);
                case EntityState.Deleted:
                    return Name + ": " + TrimToMaxLength(OriginalValue);
                case EntityState.Modified:
                    return Name + ": " + TrimToMaxLength(CurrentValue) + " (changed from " + TrimToMaxLength(OriginalValue) + ")";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string TrimToMaxLength(object value)
        {
            if (value == null)
                return "<null>";

            const int maxLength = 150;
            var toTrim = value.ToString();
            if (toTrim.Length <= maxLength)
                return toTrim;

            return toTrim.Substring(0, maxLength) + " [..]";
        }
    }
}