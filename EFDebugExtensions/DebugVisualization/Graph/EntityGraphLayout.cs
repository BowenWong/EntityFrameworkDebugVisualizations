using GraphSharp.Algorithms.OverlapRemoval;
using GraphSharp.Controls;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public class EntityGraphLayout : GraphLayout<EntityVertex, RelationEdge, EntityGraph>
    {
        public EntityGraphLayout()
        {
            OverlapRemovalAlgorithmType = "FSA";
            OverlapRemovalParameters = new OverlapRemovalParameters {HorizontalGap = 30, VerticalGap = 30};
            HighlightAlgorithmType = "None";

            DestructionTransition = null;
            CreationTransition = null;
        }
    }
}