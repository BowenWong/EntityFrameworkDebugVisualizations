using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    [DebuggerDisplay("{TypeName}: {KeyDescription}")]
    public class EntityVertex
    {
        public EntityVertex() { }

        public EntityVertex(IObjectContextAdapter context, ObjectStateEntry entry, HashSet<EntityVertex> existingVertices)
        {
            OriginalHashCode = entry.Entity.GetHashCode();
            State = entry.State;
            EntityType = entry.Entity.GetType();
            HasTemporaryKey = entry.EntityKey.IsTemporary;
            EntitySetName = entry.EntitySet.Name;

            Properties = new List<EntityProperty>();
            AddProperties(context, entry);

            Relations = new List<RelationEdge>();
            existingVertices.Add(this);
            AddRelations(context, entry, existingVertices, EntityType, this);

            Properties = Properties.OrderBy(p => p.Name).ToList();
        }

        private bool Equals(EntityVertex other)
        {
            return OriginalHashCode == other.OriginalHashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((EntityVertex) obj);
        }

        public override int GetHashCode()
        {
            return OriginalHashCode;
        }

        public static bool operator ==(EntityVertex left, EntityVertex right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityVertex left, EntityVertex right)
        {
            return !Equals(left, right);
        }

        public EntityState State { get; set; }

        public string TypeName
        {
            get { return EntityType.Name; }
        }

        public Type EntityType { get; set; }
        public bool HasTemporaryKey { get; set; }
        public string EntitySetName { get; set; }

        public int OriginalHashCode { get; set; }

        public List<EntityProperty> Properties { get; set; }

        public List<EntityProperty> ScalarProperties { get { return Properties.Where(p => !p.IsRelation).ToList(); } }
        public List<EntityProperty> RelationProperties { get { return Properties.Where(p => p.IsRelation).ToList(); } }

        public List<RelationEdge> Relations { get; set; }

        public string KeyDescription
        {
            get { return HasTemporaryKey ? "" : Properties.Where(p => p.IsKey).Aggregate("", (description, p) => p.Description); }
        }

        public string Header
        {
            get { return String.Format("{0} [{1}{2}]", TypeName, HasTemporaryKey ? "" : KeyDescription + ", ", State); }
        }

        private void AddProperties(IObjectContextAdapter context, ObjectStateEntry entry)
        {
            var dbDataRecord = entry.State != EntityState.Deleted ? entry.CurrentValues : entry.OriginalValues;
            var keyFields = GetPrimaryKeyFields(context);
            var concurrencyProperties = GetConcurrencyFields(context);
            for (int index = 0; index < dbDataRecord.FieldCount; index++)
            {
                var name = dbDataRecord.GetName(index);
                var isKey = keyFields.Any(key => key == name);
                var isConcurrencyProperty = concurrencyProperties.Any(property => property == name);
                Properties.Add(new EntityProperty(name, entry, index, isKey, isConcurrencyProperty));
            }
        }
        
        private void AddRelations(IObjectContextAdapter context, ObjectStateEntry entry, HashSet<EntityVertex> existingVertices, Type entityType, EntityVertex entityVertex)
        {
#warning this contains information about the relations: entry.RelationshipManager.GetAllRelatedEnds() => do I have something like original and current? what about the state of the relation (added, removed etc.)?
            foreach (var navigationProperty in GetNavigationProperties(context))
            {
                var currentValue = entityType.GetProperty(navigationProperty.Name).GetValue(entry.Entity);
                if (currentValue == null)
                {
                    entityVertex.Properties.Add(new EntityProperty(navigationProperty.Name, null, entityVertex.State));
                    continue;
                }

                var targetEntityType = currentValue.GetType();
                var entitySetName = GetEntitySetName(context, targetEntityType);

                if (targetEntityType.IsArray || targetEntityType.IsGenericType)
                {
                    var collection = (IEnumerable)currentValue;
                    int numElements = 0;
                    foreach (var element in collection)
                    {
                        AddRelationTarget(context, existingVertices, entitySetName, element, entityVertex, navigationProperty);
                        numElements++;
                    }

                    entityVertex.Properties.Add(new EntityProperty(navigationProperty.Name, "Collection [" + numElements + " elements]", entityVertex.State));
                }
                else
                {
                    var target = AddRelationTarget(context, existingVertices, entitySetName, currentValue, entityVertex, navigationProperty);
                    entityVertex.Properties.Add(new EntityProperty(navigationProperty.Name, "[" + target.KeyDescription + "]", entityVertex.State));
                }
            }
        }

        private EntityVertex AddRelationTarget(IObjectContextAdapter context, HashSet<EntityVertex> existingVertices, string entitySetName, object currentValue, EntityVertex entityVertex,
                                                      NavigationProperty navigationProperty)
        {
            var existingTarget = existingVertices.SingleOrDefault(v => v.OriginalHashCode == currentValue.GetHashCode());
            if (existingTarget != null)
            {
                entityVertex.Relations.Add(new RelationEdge(entityVertex, existingTarget, navigationProperty));
                return existingTarget;
            }

            ObjectStateEntry stateEntry;
            var key = context.ObjectContext.CreateEntityKey(entitySetName, currentValue);
            if (context.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(key, out stateEntry))
            {
                var target = new EntityVertex(context, stateEntry, existingVertices);
                existingVertices.Add(target);
                entityVertex.Relations.Add(new RelationEdge(entityVertex, target, navigationProperty));
                return target;
            }

            throw new NotSupportedException("Encountered related entity that is not tracked by the ObjectStateManager.");
        }

        private IEnumerable<NavigationProperty> GetNavigationProperties(IObjectContextAdapter context)
        {
            return context.ObjectContext.MetadataWorkspace
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(p => p.FullName == EntityType.FullName)
                    .NavigationProperties;
        }

        private static string GetEntitySetName(IObjectContextAdapter context, Type entityType)
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

        private List<string> GetPrimaryKeyFields(IObjectContextAdapter context)
        {
            var metadata = context.ObjectContext.MetadataWorkspace
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .SingleOrDefault(p => p.FullName == EntityType.FullName);

            if (metadata == null)
                throw new InvalidOperationException(String.Format("The type {0} is not known to the DbContext.", EntityType.FullName));

            return metadata.KeyMembers.Select(k => k.Name).ToList();
        }

        private List<string> GetConcurrencyFields(IObjectContextAdapter context)
        {
            var objType = context.ObjectContext.MetadataWorkspace.GetItems<EntityType>(DataSpace.OSpace).Single(p => p.FullName == EntityType.FullName);
            var cTypeName = (string)objType.GetType()
                    .GetProperty("CSpaceTypeName", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(objType, null);

            var conceptualType = context.ObjectContext.MetadataWorkspace.GetItems<EntityType>(DataSpace.CSpace).Single(p => p.FullName == cTypeName);
            return conceptualType.Members
                    .Where(member => member.TypeUsage.Facets.Any(facet => facet.Name == "ConcurrencyMode" && (ConcurrencyMode)facet.Value == ConcurrencyMode.Fixed))
                    .Select(member => member.Name)
                    .ToList();
        }
    }
}