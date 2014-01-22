using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
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

        public EntityProperty() { }

        public EntityProperty(string name, object currentValue, EntityState entityState)
        {
            Name = name;
            CurrentValue = currentValue;
            EntityState = entityState;
            IsRelation = true;
        }

        public EntityProperty(string name, ObjectStateEntry entry, int index, bool isKey, bool isConcurrencyProperty)
        {
            Name = name;
            CurrentValue = entry.State != EntityState.Deleted ? entry.CurrentValues.GetValue(index) : null;
            OriginalValue = entry.State != EntityState.Added ? entry.OriginalValues.GetValue(index) : null;
            IsKey = isKey;
            IsConcurrencyProperty = isConcurrencyProperty;
            EntityState = entry.State;
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
                    return Name + ": " + TrimToMaxLength(CurrentValue) + (HasValueChanged ? " (changed from " + TrimToMaxLength(OriginalValue) + ")" : "");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool HasValueChanged
        {
            get
            {
                if (OriginalValue == null && CurrentValue == null)
                    return false;

                if (OriginalValue == null ^ CurrentValue == null)
                    return true;

                return !CurrentValue.Equals(OriginalValue);
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