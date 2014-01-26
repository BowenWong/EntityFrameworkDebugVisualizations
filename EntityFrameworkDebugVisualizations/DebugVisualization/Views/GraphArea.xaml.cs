using System.Windows;
using EntityFramework.Debug.DebugVisualization.Graph;
using GraphX;
using GraphX.GraphSharp.Algorithms.OverlapRemoval;

namespace EntityFramework.Debug.DebugVisualization.Views
{
    public partial class GraphArea
    {
        public static readonly DependencyProperty GraphProperty = DependencyProperty.Register(
                "Graph", typeof(EntityGraph), typeof(GraphArea), new PropertyMetadata(default(EntityGraph), OnGraphChanged));

        private static void OnGraphChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var graphArea = ((GraphArea)d);
            if (Application.Current == null)
                graphArea.Area.EnableWinFormsHostingMode = true;
            
            graphArea.Area.LogicCore.Graph = graphArea.Graph;

            graphArea.Zoom.Zoom = 0.01;
            graphArea.Area.GenerateGraph(true);
            graphArea.Zoom.ZoomToFill();
        }

        public EntityGraph Graph
        {
            get { return (EntityGraph)GetValue(GraphProperty); }
            set { SetValue(GraphProperty, value); }
        }

        public GraphArea()
        {
            InitializeComponent();

            var logicCore = new Logic();
            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.ISOM;
            logicCore.DefaultLayoutAlgorithmParams =
                               logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.ISOM);
            //((ISOMLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            logicCore.DefaultOverlapRemovalAlgorithmParams =
                              logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
            logicCore.AsyncAlgorithmCompute = false;

            logicCore.EnableParallelEdges = true;
            logicCore.ParallelEdgeDistance = 25;

#warning try this!
            //logicCore.EdgeCurvingEnabled

            Area.LogicCore = logicCore;
        }
    }
}
