using System.Diagnostics;
using QuickGraph;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    [DebuggerDisplay("{Name}")]
    public class RelationEdge : Edge<EntityVertex>
    {
        public string Name { get; set; }

        public RelationEdge(EntityVertex source, EntityVertex target)
                : base(source, target)
        {
        }
    }
}