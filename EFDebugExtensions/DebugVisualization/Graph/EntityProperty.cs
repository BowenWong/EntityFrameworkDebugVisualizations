namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public class EntityProperty
    {
        public string Name { get; set; }
        public object OriginalValue { get; set; }
        public object CurrentValue { get; set; }

        public bool IsKey { get; set; }
        public bool IsRelation { get; set; }

        public string Description
        {
            get {  return Name + ": " + CurrentValue + (CurrentValue.Equals(OriginalValue) ? "" : " (changed from " + OriginalValue + ")"); }
        }
    }
}