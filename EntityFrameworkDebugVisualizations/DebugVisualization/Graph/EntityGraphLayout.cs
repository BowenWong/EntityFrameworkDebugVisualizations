using GraphX;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public class EntityGraphLayout : GraphArea<EntityVertex, RelationEdgeSet, EntityGraph>
    {
        public EntityGraphLayout()
        {
            //OverlapRemovalAlgorithmType = "FSA";
            //OverlapRemovalParameters = new OverlapRemovalParameters {HorizontalGap = 30, VerticalGap = 30};
            //HighlightAlgorithmType = "None";

            //DestructionTransition = null;
            //CreationTransition = null;
        }
    }
}