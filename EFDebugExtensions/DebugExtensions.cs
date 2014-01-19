using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
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
#warning what about entry.IsRelationship? are those deleted references? Einträge in Koppeltabellen?
                    .Where(e => !e.IsRelationship)
                    .ToList();
            
            foreach (var entry in stateEntries)
            {
                var existingVertex = vertices.SingleOrDefault(v => v.OriginalHashCode == entry.Entity.GetHashCode());
                if (existingVertex != null)
                    continue;

                vertices.Add(new EntityVertex(context, entry, vertices));
            }
            return vertices.ToList();
        }
    }
}