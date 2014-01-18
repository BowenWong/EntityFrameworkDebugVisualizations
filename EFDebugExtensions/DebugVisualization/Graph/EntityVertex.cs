using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public class EntityVertex
    {
        public EntityVertex()
        {
            Properties = new List<EntityProperty>();
        }

        public EntityState State { get; set; }
        public string TypeName { get; set; }
        public string FullTypeName { get; set; }
        public bool HasTemporaryKey { get; set; }
        public string EntitySetName { get; set; }

        public List<EntityProperty> Properties { get; set; }

        public string KeyDescription
        {
            get { return HasTemporaryKey ? "<added>" : Properties.Where(p => p.IsKey).Aggregate("", (description, p) => p.Description); }
        }
    }
}