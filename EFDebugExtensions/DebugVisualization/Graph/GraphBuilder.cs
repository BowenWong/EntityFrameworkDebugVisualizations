using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public static class GraphBuilder
    {
        public static List<EntityVertex> GetEntityVertices(this DbContext context)
        {
            return ((IObjectContextAdapter)context).GetEntityVertices();
        }

        public static List<EntityVertex> GetEntityVertices(this IObjectContextAdapter context)
        {
            return context.ObjectContext
                    .ObjectStateManager
                    .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged)
                    .Where(entry => entry.State != EntityState.Detached)// && entry.Entity != null)
                    .Select(entry => CreateEntityVertex(context, entry))
                    .Where(e => e != null)
                    .OrderBy(e => e.State).ThenBy(e => e.TypeName)
                    .ToList();
        }

        private static EntityVertex CreateEntityVertex(IObjectContextAdapter context, ObjectStateEntry entry)
        {
#warning what about entry.IsRelationship? are those deleted references?
            if (entry.IsRelationship)
                return null;

            var entityType = entry.Entity.GetType();
            var entityVertex = new EntityVertex
            {
                State = entry.State,
                FullTypeName = entityType.FullName,
                TypeName = entityType.Name,
                HasTemporaryKey = entry.EntityKey.IsTemporary,
                EntitySetName = entry.EntitySet.Name,
            };

            var fieldMetaData = (entry.State != EntityState.Deleted ? entry.CurrentValues : (CurrentValueRecord) entry.OriginalValues).DataRecordInfo.FieldMetadata;
            for (int index = 0; index < fieldMetaData.Count; index++)
            {
                entityVertex.Properties.Add(new EntityProperty
                {
#warning this might crash for deleted entities
                    Name = entry.CurrentValues.DataRecordInfo.FieldMetadata[index].FieldType.Name,
                    CurrentValue = entry.State != EntityState.Deleted ? entry.CurrentValues.GetValue(index) : null,
                    OriginalValue = entry.State != EntityState.Added ? entry.OriginalValues.GetValue(index) : null,
#warning this might crash for deleted entities and 'lies' for added entities with temporary keys (EntityKeyValues is null)
                    IsKey = !entry.EntityKey.IsTemporary && entry.EntityKey.EntityKeyValues.Any(key => key.Key == entry.CurrentValues.DataRecordInfo.FieldMetadata[index].FieldType.Name)
                });
            }

#warning navigation properties should be kept separately from other properties as I need to construct edges from them
            foreach (var navigationProperty in context.GetNavigationPropertiesForType(entityType))
            {
#warning this contains information about the relations => do I have something like original and current? what about the name? Rels = entry.RelationshipManager.GetAllRelatedEnds()
                entityVertex.Properties.Add(new EntityProperty
                {
                    Name = navigationProperty.Name,
                    IsRelation = true,
#warning this needs to find or construct the destination EntityVertex
                    CurrentValue = entityType.GetProperty(navigationProperty.Name)
#warning can I say something about the state of the relation (added, removed etc.)?
                });
            }

            entityVertex.Properties = entityVertex.Properties.OrderBy(p => p.Name).ToList();
            return entityVertex;
        }

        private static IEnumerable<NavigationProperty> GetNavigationPropertiesForType(this IObjectContextAdapter context, Type entityType)
        {
            return context.ObjectContext.MetadataWorkspace
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(p => p.FullName == entityType.FullName)
                    .NavigationProperties;
        }
    }
}