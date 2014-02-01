using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Windows;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public class Relation
    {
        public Relation(string name, string multiplicity, OperationAction deleteBehavior, EntityState state)
        {
            Name = name;
            Multiplicity = multiplicity;
            DeleteBehavior = deleteBehavior;
            State = state;
        }

        public string Name { get; set; }

        public string Multiplicity { get; set; }

        public OperationAction DeleteBehavior { get; set; }

        public EntityState State { get; set; }

        public Visibility DisplayCascadedDelete { get { return DeleteBehavior == OperationAction.Cascade ? Visibility.Visible : Visibility.Collapsed; } }
    }
}