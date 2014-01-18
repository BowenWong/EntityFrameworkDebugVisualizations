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
            var existingVertices = new HashSet<EntityVertex>();
            var stateEntries = context.ObjectContext
                    .ObjectStateManager
                    .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged)
                    .Where(entry => entry.State != EntityState.Detached);

            foreach (var entry in stateEntries)
                CreateEntityVertex(context, entry, existingVertices);

            return existingVertices.ToList();
        }

        private static EntityVertex CreateEntityVertex(IObjectContextAdapter context, ObjectStateEntry entry, HashSet<EntityVertex> existingVertices)
        {
#warning what about entry.IsRelationship? are those deleted references? Einträge in Koppeltabellen?
            if (entry.IsRelationship)
                return null;

            var entityType = entry.Entity.GetType();
            var entityVertex = new EntityVertex
            {
                OriginalHashCode = entry.Entity.GetHashCode(),
                State = entry.State,
                FullTypeName = entityType.FullName,
                TypeName = entityType.Name,
                HasTemporaryKey = entry.EntityKey.IsTemporary,
                EntitySetName = entry.EntitySet.Name,
            };

            if (!existingVertices.Add(entityVertex))
                return existingVertices.Single(v => v == entityVertex);

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
            entityVertex.Properties = entityVertex.Properties.OrderBy(p => p.Name).ToList();

#warning this contains information about the relations: entry.RelationshipManager.GetAllRelatedEnds() => do I have something like original and current? what about the state of the relation (added, removed etc.)?
            foreach (var navigationProperty in context.GetNavigationPropertiesForType(entityType))
            {
                var currentValue = entityType.GetProperty(navigationProperty.Name).GetValue(entry.Entity);
                if (currentValue == null)
                {
#warning what to do about 'empty' relations? how about adding them to properties as well (with target.KeyDescription as values)?
                    continue;
                }

                var entitySetName = context.GetEntitySetName(currentValue.GetType());
                var key = context.ObjectContext.CreateEntityKey(entitySetName, currentValue);
                var targetEntity = context.ObjectContext
                        .ObjectStateManager
                        .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged)
                        .SingleOrDefault(e => e.EntityKey == key);

                EntityVertex target = CreateEntityVertex(context, targetEntity, existingVertices);
                entityVertex.AddRelation(navigationProperty.Name, target);
            }

            return entityVertex;
        }

        private static IEnumerable<NavigationProperty> GetNavigationPropertiesForType(this IObjectContextAdapter context, Type entityType)
        {
            return context.ObjectContext.MetadataWorkspace
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(p => p.FullName == entityType.FullName)
                    .NavigationProperties;
        }

        private static string GetEntitySetName(this IObjectContextAdapter context, Type entityType)
        {
            Type type = entityType;
            EntitySetBase set = null;

            while (set == null && type != null)
            {
                set = context.ObjectContext.MetadataWorkspace
                        .GetEntityContainer(context.ObjectContext.DefaultContainerName, DataSpace.CSpace)
                        .EntitySets
                        .FirstOrDefault(item => item.ElementType.Name.Equals(type.Name));

                type = type.BaseType;
            }

            return set != null ? set.Name : null;
        }
    }
}