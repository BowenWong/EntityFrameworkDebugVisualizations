using System;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using QuickGraph;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    [DebuggerDisplay("{Name} ({State})")]
    public class RelationEdge : Edge<EntityVertex>
    {
        public string Name { get; set; }

        public string Multiplicity { get; set; }

        public OperationAction DeleteBehavior { get; set; }

        public EntityState State { get; set; }

        public string TooltipText
        {
            get { return string.Format("{0} ({1}){2}\nState: {3}", Name, Multiplicity, DeleteBehavior == OperationAction.Cascade ? " (cascaded delete)" : "", State); }
        }

        public RelationEdge(EntityVertex source, EntityVertex target, NavigationProperty navigationProperty)
                : base(source, target)
        {
            // this is required for json deserialization
            if (navigationProperty == null) 
                return;

            State = EntityState.Unchanged;
            DeleteBehavior = navigationProperty.ToEndMember.DeleteBehavior;
            Multiplicity = GetHumanReadableMultiplicity(navigationProperty);
            Name = navigationProperty.Name;
        }

        private string GetHumanReadableMultiplicity(NavigationProperty navigationProperty)
        {
            return string.Format("{0} {1} to {2} {3}",
                                 HumanReadableMultiplicity(navigationProperty.FromEndMember.RelationshipMultiplicity),
                                 GetEntityName(Source, navigationProperty.FromEndMember.RelationshipMultiplicity),
                                 HumanReadableMultiplicity(navigationProperty.ToEndMember.RelationshipMultiplicity),
                                 GetEntityName(Target, navigationProperty.ToEndMember.RelationshipMultiplicity));
        }

        private static string GetEntityName(EntityVertex entityVertex, RelationshipMultiplicity multiplicity)
        {
            return multiplicity == RelationshipMultiplicity.One ? entityVertex.TypeName : entityVertex.EntitySetName;
        }

        private static string HumanReadableMultiplicity(RelationshipMultiplicity multiplicity)
        {
            switch (multiplicity)
            {
                case RelationshipMultiplicity.ZeroOrOne:
                    return "zero or one";
                case RelationshipMultiplicity.One:
                    return "one";
                case RelationshipMultiplicity.Many:
                    return "many";
                default:
                    throw new ArgumentOutOfRangeException("multiplicity");
            }
        }
    }
}