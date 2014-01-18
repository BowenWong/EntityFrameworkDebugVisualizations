using QuickGraph;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public class EntityGraph : BidirectionalGraph<EntityVertex, RelationEdge>
    {
        public EntityGraph()
                : base(allowParallelEdges: true)
        {
        }
    }
}