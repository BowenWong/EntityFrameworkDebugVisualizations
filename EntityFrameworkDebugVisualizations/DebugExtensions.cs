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

            var relationStateEntries = context
                    .ObjectContext
                    .ObjectStateManager
                    .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Unchanged)
                    .Where(entry => entry.IsRelationship && entry.State != EntityState.Unchanged) // RelationEdges are unchanged by default, so no need to update
                    .ToList();

            foreach (var entry in stateEntries)
            {
                if (vertices.Any(v => v.OriginalHashCode == entry.Entity.GetHashCode()))
                    continue;

                var entityVertex = new EntityVertex(context.ObjectContext.MetadataWorkspace, entry);
                entityVertex.AddRelations(context, entry, vertices);
            }

            foreach (var relationStateEntry in relationStateEntries)
            {
                var relationKeys = GetEntityKeysForRelation(relationStateEntry);
                foreach (var vertex in vertices)
                {
                    var associationSetEnd = vertex.GetAssociationEnd(relationStateEntry, relationKeys);
                    if (associationSetEnd == null)
                        continue;

                    var navigationProperty = vertex
                            .GetNavigationProperties(context)
                            .SingleOrDefault(n => n.FromEndMember.Name == associationSetEnd.CorrespondingAssociationEndMember.Name);
                    if (navigationProperty == null)
                        continue;

                    var targetKey = relationKeys.Single(k => k != vertex.EntityKey);
                    var target = vertices.SingleOrDefault(v => v.EntityKey == targetKey);
                    if (target == null)
                        continue;

                    var matchingRelation = vertex.Relations.SingleOrDefault(r => r.Relations[0].Name == navigationProperty.Name && r.Target.EntityKey == target.EntityKey);
                    if (matchingRelation != null)
                        matchingRelation.Relations[0].State = relationStateEntry.State;
                    else if (relationStateEntry.State == EntityState.Deleted)
                        vertex.Relations.Add(new RelationEdgeSet(vertex, target, navigationProperty, EntityState.Deleted));
                }
            }

            return vertices.ToList();
        }

        private static AssociationSetEnd GetAssociationEnd(this EntityVertex vertex, ObjectStateEntry relation, List<EntityKey> relationKeys)
        {
            var keyIndex = relationKeys.IndexOf(vertex.EntityKey);
            if (keyIndex < 0)
                return null;

            var associationSetEnd = ((AssociationSet)relation.EntitySet).AssociationSetEnds[keyIndex];
            return associationSetEnd.EntitySet.Name == vertex.EntitySetName ? associationSetEnd : null;
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