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

        public VisualizerViewModel(List<EntityVertex> vertices)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            _vertices = vertices;
            EntityTypes = _vertices.Select(v => v.TypeName).Distinct().Select(typeName => new EntityTypeFilterViewModel(typeName, UpdateGraph)).ToList();

            UpdateGraph();
        }

        private void UpdateGraph()
        {
            Graph = new EntityGraph();

            var typeWhitelist = EntityTypes.Where(e => e.IsSelected).Select(e => e.TypeName).ToList();
            var filteredVertices = _vertices
                    .Where(v => _showAddedEntities || v.State != EntityState.Added)
                    .Where(v => _showDeletedEntities || v.State != EntityState.Deleted)
                    .Where(v => _showModifiedEntities || v.State != EntityState.Modified)
                    .Where(v => _showUnchangedEntities || v.State != EntityState.Unchanged)
                    .Where(v => typeWhitelist.Contains(v.TypeName))
                    .ToList();

            Graph.AddVertexRange(filteredVertices);
#warning re-implement filtering using Graph.RemoveVertexIf()

            var filteredEdges = filteredVertices.SelectMany(v => v.Relations).Where(r => filteredVertices.Contains(r.Target));
            Graph.AddEdgeRange(filteredEdges);
#warning re-implement filtering using Graph.RemoveEdgeIf()
        }
    }
}