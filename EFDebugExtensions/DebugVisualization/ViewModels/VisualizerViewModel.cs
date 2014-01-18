using System.ComponentModel;
using System.Runtime.CompilerServices;
using EntityFramework.Debug.DebugVisualization.Graph;

namespace EntityFramework.Debug.DebugVisualization.ViewModels
{
    public class VisualizerViewModel : INotifyPropertyChanged
    {
        private EntityGraph _graph;
        public EntityGraph Graph
        {
            get { return _graph; }
            set { _graph = value; OnPropertyChanged(); }
        }

        public VisualizerViewModel()
        {
            Graph = new EntityGraph();

            var test1 = new EntityVertex {TypeName = "Test 1"};
            Graph.AddVertex(test1);

            var test2 = new EntityVertex {TypeName = "Test 2"};
            Graph.AddVertex(test2);

            Graph.AddEdge(new RelationEdge(test1, test2));
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}