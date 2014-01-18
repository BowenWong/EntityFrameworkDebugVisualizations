using System.Collections.Generic;
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

        public VisualizerViewModel(List<EntityVertex> vertices)
        {
            Graph = new EntityGraph();

            foreach (var vertex in vertices)
                Graph.AddVertex(vertex);

            foreach (var vertex in vertices)
                foreach (var relation in vertex.Relations)
                    Graph.AddEdge(relation);
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