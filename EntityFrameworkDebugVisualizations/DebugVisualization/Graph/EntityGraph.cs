using QuickGraph;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public class EntityGraph : BidirectionalGraph<EntityVertex, RelationEdgeSet>
    {
        public EntityGraph()
                : base(false)
        {
        }
    }
}