using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Diagnostics;
using System.Linq;
using QuickGraph;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    [DebuggerDisplay("{Name} ({State})")]
    public class RelationEdgeSet : Edge<EntityVertex>
    {
        public List<Relation> Relations { get; set; }

        public bool ContainsMultipleRelations
        {
            get { return Relations.Count > 1; }
        }

        public string TooltipText
        {
            get { return string.Join("\n\n", Relations.Select(r => r.TooltipText)); }
        }

        public EntityState State
        {
            get
            {
                if (Relations.Any(r => r.State == EntityState.Deleted))
                    return EntityState.Deleted;
                if (Relations.Any(r => r.State == EntityState.Added))
                    return EntityState.Added;
                return EntityState.Unchanged;
            }
        }

        public RelationEdgeSet(EntityVertex source, EntityVertex target, NavigationProperty navigationProperty, EntityState state = EntityState.Unchanged)
                : base(source, target)
        {
            Relations = new List<Relation>();

            // this is required for json deserialization
            if (navigationProperty == null) 
                return;

            var multiplicity = GetHumanReadableMultiplicity(navigationProperty);
            Relations.Add(new Relation(navigationProperty.Name, multiplicity, navigationProperty.ToEndMember.DeleteBehavior, state));
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

        public void Merge(RelationEdgeSet parallelRelation)
        {

        }
    }
}