using GraphSharp.Controls;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public class EntityGraphLayout : GraphLayout<EntityVertex, RelationEdge, EntityGraph>
    {
        public EntityGraphLayout()
        {
            LayoutAlgorithmType = "ISOM"; // "EfficientSugiyama"
            OverlapRemovalAlgorithmType = "FSA";
            HighlightAlgorithmType = "None";

            DestructionTransition = null;
            CreationTransition = null;
        }
    }
}