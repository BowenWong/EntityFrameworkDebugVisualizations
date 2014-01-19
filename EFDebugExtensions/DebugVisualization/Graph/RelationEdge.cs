using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using QuickGraph;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    [DebuggerDisplay("{Name}")]
    public class RelationEdge : Edge<EntityVertex>
    {
        public string Name { get; set; }

        public string Multiplicity { get; set; }

        public OperationAction DeleteBehavior { get; set; }

        public string TooltipText
        {
            get { return string.Format("{0} ({1}){2}", Name, Multiplicity, DeleteBehavior == OperationAction.Cascade ? " (cascaded delete)" : ""); }
        }

        public RelationEdge(EntityVertex source, EntityVertex target)
                : base(source, target)
        {
        }
    }
}