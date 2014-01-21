using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using EntityFramework.Debug.DebugVisualization;
using EntityFramework.Debug.DebugVisualization.Graph;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace EntityFramework.Debug
{
    public static class DebugExtensions
    {
        public static string DumpTrackedEntities(this IObjectContextAdapter context)
        {
            var builder = new StringBuilder();
            foreach (var stateGroup in context.GetEntityVertices().GroupBy(e => e.State))
            {
                builder.AppendLine(stateGroup.Key.ToString());
                builder.AppendLine("----------------");

                foreach (var entity in stateGroup)
                {
                    builder.AppendFormat("{0} (# {1})", entity.EntityType.Name, entity.OriginalHashCode).AppendLine();

                    builder.Indent(4).AppendLine("Properties:");
                    foreach (var property in entity.ScalarProperties)
                        builder.Indent(8).AppendLine(property.Description);

                    builder.Indent(4).AppendLine("Relations:");
                    foreach (var property in entity.RelationProperties)
                        builder.Indent(8).AppendLine(property.Description);

                    builder.AppendLine();
                }
            }
            return builder.ToString();
        }

        private static StringBuilder Indent(this StringBuilder builder, int numSpaces)
        {
            for (int i = 0; i < numSpaces; i++)
                builder.Append(" ");
            return builder;
        }
        
        public static void ShowVisualizer(this IObjectContextAdapter context)
        {
            new VisualizerDevelopmentHost(context, typeof(ContextDebuggerVisualizer), typeof(ContextVisualizerObjectSource)).ShowVisualizer();
        }

        public static List<EntityVertex> GetEntityVertices(this IObjectContextAdapter context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.ObjectContext.DetectChanges();

            var vertices = new HashSet<EntityVertex>();
            var stateEntries = context
                    .ObjectContext
                    .ObjectStateManager
                    .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged)
                    .Where(entry => !entry.IsRelationship && entry.Entity != null)
                    .ToList();

            foreach (var entry in stateEntries)
            {
                if (vertices.Any(v => v.OriginalHashCode == entry.Entity.GetHashCode()))
                    continue;

                vertices.Add(new EntityVertex(context, entry, vertices));
            }

#warning clean the following code up, it's shit!! I think it'd be best to integrate it into the vertex construction code above..
            var relations = context
                    .ObjectContext
                    .ObjectStateManager
                    .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Unchanged)
                    .Where(entry => entry.IsRelationship)
                    .ToList();

            foreach (var relationStateEntry in relations)
            {
                // RelationEdges are unchanged by default, so no need to update anything
                if (relationStateEntry.State == EntityState.Unchanged)
                    continue;

                var relationKeys = GetEntityKeysForRelation(relationStateEntry);
                foreach (var vertex in vertices)
                {
                    var matchingRelationKey = relationKeys.SingleOrDefault(k => k == vertex.EntityKey);
                    if (matchingRelationKey == null)
                        continue;

                    var associationSet = (AssociationSet)relationStateEntry.EntitySet;
                    var endMember = associationSet.AssociationSetEnds[relationKeys.IndexOf(matchingRelationKey)];
                    if (endMember.EntitySet.Name != vertex.EntitySetName)
                        continue;

                    foreach (var navigationProperty in vertex.GetNavigationProperties(context).Where(n => n.FromEndMember.Name == endMember.CorrespondingAssociationEndMember.Name))
                    {
                        var targetKey = relationKeys.Single(k => k != vertex.EntityKey);
                        var target = vertices.SingleOrDefault(v => v.EntityKey == targetKey);
                        if (target == null)
                            continue;

                        var matchingRelation = vertex.Relations.SingleOrDefault(r => r.Name == navigationProperty.Name && r.Target.EntityKey == target.EntityKey);
                        if (relationStateEntry.State == EntityState.Deleted)
                            vertex.Relations.Add(new RelationEdge(vertex, target, navigationProperty) { State = EntityState.Deleted });
                        else if (matchingRelation != null)
                            matchingRelation.State = relationStateEntry.State;
                    }
                }
            }

            return vertices.ToList();
        }

        private static List<EntityKey> GetEntityKeysForRelation(ObjectStateEntry relation)
        {
            var keys = new List<EntityKey>();
            var properties = relation
                    .GetType()
                    .GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic);
            
            var key0Property = properties.SingleOrDefault(p => p.Name == "Key0");
            if (key0Property != null)
                keys.Add((EntityKey)key0Property.GetValue(relation));

            var key1Property = properties.SingleOrDefault(p => p.Name == "Key1");
            if (key1Property != null)
                keys.Add((EntityKey)key1Property.GetValue(relation));

            if (keys.Count != 2)
                throw new ArgumentException("A relation should have two keys.");

            return keys;
        }
    }
}