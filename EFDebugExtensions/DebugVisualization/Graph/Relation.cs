using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public class Relation
    {
        public Relation(string name, string multiplicity, OperationAction deleteBehavior, EntityState state)
        {
            Name = name;
            Multiplicity = multiplicity;
            DeleteBehavior = deleteBehavior;
            State = state;
        }

        public string Name { get; set; }

        public string Multiplicity { get; set; }

        public OperationAction DeleteBehavior { get; set; }

        public EntityState State { get; set; }

        public string TooltipText
        {
            get { return string.Format("{0} ({1}){2}\nState: {3}", Name, Multiplicity, DeleteBehavior == OperationAction.Cascade ? " (cascaded delete)" : "", State); }
        }
    }
}