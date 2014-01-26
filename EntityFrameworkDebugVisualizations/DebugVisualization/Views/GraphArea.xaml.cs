using System.Windows;
using EntityFramework.Debug.DebugVisualization.Graph;
using GraphX;
using GraphX.GraphSharp.Algorithms.Layout.Simple.FDP;
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
            graphArea.Area.GenerateGraph(true);
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
            //This property sets layout algorithm that will be used to calculate vertices positions
            //Different algorithms uses different values and some of them uses edge Weight property.
            logicCore.DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.KK;
            //Now we can set optional parameters using AlgorithmFactory
            logicCore.DefaultLayoutAlgorithmParams =
                               logicCore.AlgorithmFactory.CreateLayoutParameters(LayoutAlgorithmTypeEnum.KK);
            //Unfortunately to change algo parameters you need to specify params type which is different for every algorithm.
            ((KKLayoutParameters)logicCore.DefaultLayoutAlgorithmParams).MaxIterations = 100;

            //This property sets vertex overlap removal algorithm.
            //Such algorithms help to arrange vertices in the layout so no one overlaps each other.
            logicCore.DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA;
            //Setup optional params
            logicCore.DefaultOverlapRemovalAlgorithmParams =
                              logicCore.AlgorithmFactory.CreateOverlapRemovalParameters(OverlapRemovalAlgorithmTypeEnum.FSA);
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).HorizontalGap = 50;
            ((OverlapRemovalParameters)logicCore.DefaultOverlapRemovalAlgorithmParams).VerticalGap = 50;

            //This property sets edge routing algorithm that is used to build route paths according to algorithm logic.
            //For ex., SimpleER algorithm will try to set edge paths around vertices so no edge will intersect any vertex.
            logicCore.DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;

            //This property sets async algorithms computation so methods like: Area.RelayoutGraph() and Area.GenerateGraph()
            //will run async with the UI thread. Completion of the specified methods can be catched by corresponding events:
            //Area.RelayoutFinished and Area.GenerateGraphFinished.
            logicCore.AsyncAlgorithmCompute = false;

            //Finally assign logic core to GraphArea object
            Area.LogicCore = logicCore;
        }
    }
}
