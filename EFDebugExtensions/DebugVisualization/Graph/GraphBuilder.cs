using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    public static class GraphBuilder
    {
        public static List<EntityVertex> GetEntityVertices(this ObjectContext context)
        {
            var existingVertices = new HashSet<EntityVertex>();
            var stateEntries = context
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

            var dbDataRecord = entry.State != EntityState.Deleted ? entry.CurrentValues : entry.OriginalValues;
            var keyFields = context.GetPrimaryKeyFieldsForType(entityType);
            var concurrencyProperties = context.GetConcurrencyFieldsForType(entityType);
            for (int index = 0; index < dbDataRecord.FieldCount; index++)
            {
                entityVertex.Properties.Add(new EntityProperty
                {
                    Name = dbDataRecord.GetName(index),
                    CurrentValue = entry.State != EntityState.Deleted ? entry.CurrentValues.GetValue(index) : null,
                    OriginalValue = entry.State != EntityState.Added ? entry.OriginalValues.GetValue(index) : null,
                    IsKey = keyFields.Any(key => key == dbDataRecord.GetName(index)),
                    IsConcurrencyProperty = concurrencyProperties.Any(property => property == dbDataRecord.GetName(index)),
                    EntityState = entityVertex.State,
                });
            }

#warning this contains information about the relations: entry.RelationshipManager.GetAllRelatedEnds() => do I have something like original and current? what about the state of the relation (added, removed etc.)?
            foreach (var navigationProperty in context.GetNavigationPropertiesForType(entityType))
            {
                var currentValue = entityType.GetProperty(navigationProperty.Name).GetValue(entry.Entity);
                if (currentValue == null)
                {
                    entityVertex.Properties.Add(CreateRelationProperty(navigationProperty.Name, null, entityVertex.State));
                    continue;
                }

                var targetEntityType = currentValue.GetType();
                var entitySetName = context.GetEntitySetName(targetEntityType);

                if (targetEntityType.IsArray || targetEntityType.IsGenericType)
                {
                    var collection = (IEnumerable) currentValue;
                    int numElements = 0;
                    foreach (var element in collection)
                    {
                        AddRelationTarget(context, existingVertices, entitySetName, element, entityVertex, navigationProperty);
                        numElements++;
                    }

                    entityVertex.Properties.Add(CreateRelationProperty(navigationProperty.Name, "Collection [" + numElements + " elements]", entityVertex.State));

                }
                else
                {
                    var target = AddRelationTarget(context, existingVertices, entitySetName, currentValue, entityVertex, navigationProperty);
                    entityVertex.Properties.Add(CreateRelationProperty(navigationProperty.Name, "[" + target.KeyDescription + "]", entityVertex.State));
                }
            }

            entityVertex.Properties = entityVertex.Properties.OrderBy(p => p.Name).ToList();
            return entityVertex;
        }

        private static EntityVertex AddRelationTarget(IObjectContextAdapter context, HashSet<EntityVertex> existingVertices, string entitySetName, object currentValue, EntityVertex entityVertex,
                                                      NavigationProperty navigationProperty)
        {
            var key = context.ObjectContext.CreateEntityKey(entitySetName, currentValue);
            var targetEntity = context.ObjectContext
                    .ObjectStateManager
                    .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged)
                    .SingleOrDefault(e => e.EntityKey == key);

            EntityVertex target = CreateEntityVertex(context, targetEntity, existingVertices);
            entityVertex.AddRelation(navigationProperty.Name, target);
            return target;
        }

        private static EntityProperty CreateRelationProperty(string name, object currentValue, EntityState entityState)
        {
            return new EntityProperty
            {
                Name = name,
                CurrentValue = currentValue,
                EntityState = entityState,
                IsRelation = true,
            };
        }

        private static IEnumerable<NavigationProperty> GetNavigationPropertiesForType(this IObjectContextAdapter context, Type entityType)
        {
            return context.ObjectContext.MetadataWorkspace
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(p => p.FullName == entityType.FullName)
                    .NavigationProperties;
        }

        private static List<string> GetPrimaryKeyFieldsForType(this IObjectContextAdapter context, Type entityType)
        {
            var metadata = context.ObjectContext.MetadataWorkspace
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .SingleOrDefault(p => p.FullName == entityType.FullName);

            if (metadata == null)
                throw new InvalidOperationException(String.Format("The type {0} is not known to the DbContext.", entityType.FullName));

            return metadata.KeyMembers.Select(k => k.Name).ToList();
        }

        private static List<string> GetConcurrencyFieldsForType(this IObjectContextAdapter context, Type entityType)
        {
            var objType = context.ObjectContext.MetadataWorkspace.GetItems<EntityType>(DataSpace.OSpace).Single(p => p.FullName == entityType.FullName);
            var cTypeName = (string) objType.GetType()
                    .GetProperty("CSpaceTypeName", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(objType, null);

            var conceptualType = context.ObjectContext.MetadataWorkspace.GetItems<EntityType>(DataSpace.CSpace).Single(p => p.FullName == cTypeName);
            return conceptualType.Members
                    .Where(member => member.TypeUsage.Facets.Any(facet => facet.Name == "ConcurrencyMode" && (ConcurrencyMode) facet.Value == ConcurrencyMode.Fixed))
                    .Select(member => member.Name)
                    .ToList();
        }

        private static string GetEntitySetName(this IObjectContextAdapter context, Type entityType)
        {
            Type type = entityType;
            if (type.IsArray)
                type = type.GetElementType();
            if (type.IsGenericType)
                type = type.GetGenericArguments()[0];

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