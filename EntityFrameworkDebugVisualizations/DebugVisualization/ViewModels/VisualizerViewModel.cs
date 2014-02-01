using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using EntityFramework.Debug.DebugVisualization.Graph;

namespace EntityFramework.Debug.DebugVisualization.ViewModels
{
    public class VisualizerViewModel : AViewModel
    {
        private EntityGraph _graph;
        public EntityGraph Graph
        {
            get { return _graph; }
            set { _graph = value; OnPropertyChanged(); }
        }

        private bool _showAddedEntities = true;
        public bool ShowAddedEntities
        {
            get { return _showAddedEntities; }
            set { _showAddedEntities = value; OnPropertyChanged(); UpdateGraph(); }
        }

        private bool _showDeletedEntities = true;
        public bool ShowDeletedEntities
        {
            get { return _showDeletedEntities; }
            set { _showDeletedEntities = value; OnPropertyChanged(); UpdateGraph(); }
        }

        private bool _showModifiedEntities = true;
        public bool ShowModifiedEntities
        {
            get { return _showModifiedEntities; }
            set { _showModifiedEntities = value; OnPropertyChanged(); UpdateGraph(); }
        }

        private bool _showUnchangedEntities = true;
        public bool ShowUnchangedEntities
        {
            get { return _showUnchangedEntities; }
            set { _showUnchangedEntities = value; OnPropertyChanged(); UpdateGraph(); }
        }

        public List<EntityTypeFilterViewModel> EntityTypes { get; set; }

        private readonly List<EntityVertex> _vertices;
        private List<EntityVertex> _currentlyVisibleVertices;

        private readonly List<string> _algorithmTypes;
        private string _selectedAlgorithmType = "BoundedFR";
        public List<string> AlgorithmTypes
        {
            get { return _algorithmTypes; }
        }

        public string SelectedAlgorithmType
        {
            get { return _selectedAlgorithmType; }
            set { _selectedAlgorithmType = value; OnPropertyChanged(); }
        }

        public VisualizerViewModel(List<EntityVertex> vertices)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            _vertices = vertices;
            EntityTypes = _vertices.Select(v => v.TypeName).Distinct().Select(typeName => new EntityTypeFilterViewModel(typeName, UpdateGraph)).ToList();
            _algorithmTypes = new EntityGraphLayout().LayoutAlgorithmFactory.AlgorithmTypes.ToList();

            Graph = new EntityGraph();
            UpdateGraph();
        }

        private void UpdateGraph()
        {
            var typeWhitelist = EntityTypes.Where(e => e.IsSelected).Select(e => e.TypeName).ToList();
            var filteredVertices = _vertices
                    .Where(v => _showAddedEntities || v.State != EntityState.Added)
                    .Where(v => _showDeletedEntities || v.State != EntityState.Deleted)
                    .Where(v => _showModifiedEntities || v.State != EntityState.Modified)
                    .Where(v => _showUnchangedEntities || v.State != EntityState.Unchanged)
                    .Where(v => typeWhitelist.Contains(v.TypeName))
                    .ToList();

            var toAdd = filteredVertices.Except(_currentlyVisibleVertices ?? new List<EntityVertex>()).ToList();
            var toRemove = (_currentlyVisibleVertices ?? new List<EntityVertex>()).Where(v => !filteredVertices.Contains(v)).ToList();

            Graph.RemoveVertexIf(v => toRemove.Contains(v));
            Graph.AddVertexRange(toAdd);

            var filteredEdges = toAdd.SelectMany(v => v.Relations).Where(r => toAdd.Contains(r.Target));
            Graph.AddEdgeRange(filteredEdges);

            Graph.RemoveEdgeIf(edge => toRemove.Contains(edge.Source) || toRemove.Contains(edge.Target));

            _currentlyVisibleVertices = filteredVertices;
        }
    }
}