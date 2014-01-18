using QuickGraph;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public class RelationEdge : Edge<EntityVertex>
    {
        public string Name { get; set; }

        public RelationEdge(EntityVertex source, EntityVertex target)
                : base(source, target)
        {
        }
    }
}