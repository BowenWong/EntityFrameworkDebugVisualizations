using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    [DebuggerDisplay("{TypeName}: {KeyDescription}")]
    public class EntityVertex
    {
        public EntityVertex()
        {
            Properties = new List<EntityProperty>();
            Relations = new List<RelationEdge>();
        }

        private bool Equals(EntityVertex other)
        {
            return OriginalHashCode == other.OriginalHashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((EntityVertex) obj);
        }

        public override int GetHashCode()
        {
            return OriginalHashCode;
        }

        public static bool operator ==(EntityVertex left, EntityVertex right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityVertex left, EntityVertex right)
        {
            return !Equals(left, right);
        }

        public EntityState State { get; set; }
        public string TypeName { get; set; }
        public string FullTypeName { get; set; }
        public bool HasTemporaryKey { get; set; }
        public string EntitySetName { get; set; }

        public int OriginalHashCode { get; set; }

        public List<EntityProperty> Properties { get; set; }
        public List<RelationEdge> Relations { get; set; }

        public string KeyDescription
        {
            get { return HasTemporaryKey ? "<added>" : Properties.Where(p => p.IsKey).Aggregate("", (description, p) => p.Description); }
        }

        public void AddRelation(string relationName, EntityVertex target)
        {
            Relations.Add(new RelationEdge(this, target){Name = relationName});
        }
    }
}