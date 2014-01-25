using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using GraphX;

namespace EntityFramework.Debug.DebugVisualization.Graph
{
    [DebuggerDisplay("{TypeName}: {KeyDescription} ({State})")]
    public class EntityVertex : VertexBase
    {
        public EntityVertex() { }

        public EntityVertex(MetadataWorkspace metadataWorkspace, ObjectStateEntry entry)
        {
            OriginalHashCode = entry.Entity.GetHashCode();
            State = entry.State;
            EntityType = entry.Entity.GetType();
            HasTemporaryKey = entry.EntityKey.IsTemporary;
            EntitySetName = entry.EntitySet.Name;
            EntityKey = entry.EntityKey;

            Properties = new List<EntityProperty>();
            AddProperties(metadataWorkspace, entry);

            Relations = new List<RelationEdgeSet>();
        }

        public EntityState State { get; set; }

        public string TypeName
        {
            get { return EntityType.Name; }
        }

        public Type EntityType { get; set; }
        public bool HasTemporaryKey { get; set; }
        public string EntitySetName { get; set; }
        
        public EntityKey EntityKey { get; set; }
        public int OriginalHashCode { get; set; }

        public List<EntityProperty> Properties { get; set; }

        public IEnumerable<EntityProperty> ScalarProperties { get { return Properties.Where(p => !p.IsRelation).ToList(); } }
        public IEnumerable<EntityProperty> RelationProperties { get { return Properties.Where(p => p.IsRelation).ToList(); } }

        public List<RelationEdgeSet> Relations { get; set; }

        public string KeyDescription
        {
            get { return HasTemporaryKey ? "" : String.Join(", ", Properties.Where(p => p.IsKey).Select(p => p.Description)); }
        }

        public string Header
        {
            get { return String.Format("{0} [{1}{2}]", TypeName, HasTemporaryKey ? "" : KeyDescription + ", ", State); }
        }

        public bool HasChanged
        {
            get { return State != EntityState.Unchanged; }
        }

        private void AddProperties(MetadataWorkspace metadataWorkspace, ObjectStateEntry entry)
        {
            var dbDataRecord = entry.State != EntityState.Deleted ? entry.CurrentValues : entry.OriginalValues;
            var keyFields = GetPrimaryKeyFields(metadataWorkspace);
            var concurrencyProperties = GetConcurrencyFields(metadataWorkspace);
            for (int index = 0; index < dbDataRecord.FieldCount; index++)
            {
                var name = dbDataRecord.GetName(index);
                var isKey = keyFields.Any(key => key == name);
                var isConcurrencyProperty = concurrencyProperties.Any(property => property == name);
                Properties.Add(new EntityProperty(name, entry, index, isKey, isConcurrencyProperty));
            }
        }

        internal void AddRelations(IObjectContextAdapter context, ObjectStateEntry entry, HashSet<EntityVertex> existingVertices)
        {
            existingVertices.Add(this);

            foreach (var navigationProperty in GetNavigationProperties(context))
            {
                var targetPropertyInfo = EntityType.GetProperty(navigationProperty.Name);
                var isCollectionType = IsCollectionType(targetPropertyInfo.PropertyType);
                var currentValue = targetPropertyInfo.GetValue(entry.Entity);

                var targets = EnumerateCurrentValue(currentValue, isCollectionType)
                        .Select(current => existingVertices.SingleOrDefault(v => v.OriginalHashCode == current.GetHashCode())
                                           ?? CreateVertexFromEntity(current, context, existingVertices))
                        .ToList();

                CreateRelationProperty(navigationProperty, currentValue, targets, isCollectionType);

                foreach (var target in targets)
                    Relations.Add(new RelationEdgeSet(this, target, navigationProperty));
            }
            Properties = Properties.OrderBy(p => p.Name).ToList();
        }

        private void CreateRelationProperty(NavigationProperty navigationProperty, object currentValue, IReadOnlyList<EntityVertex> targets, bool isCollectionType)
        {
            var relationPropertyValue = currentValue != null ? GetRelationPropertyValue(targets, isCollectionType) : null;
            Properties.Add(new EntityProperty(navigationProperty.Name, relationPropertyValue, State));
        }

        private static EntityVertex CreateVertexFromEntity(object entity, IObjectContextAdapter context, HashSet<EntityVertex> existingVertices)
        {
            ObjectStateEntry stateEntry;
            if (!context.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out stateEntry))
                throw new NotSupportedException("Encountered related entity that is not tracked by the ObjectStateManager.");

            var target = new EntityVertex(context.ObjectContext.MetadataWorkspace, stateEntry);
            target.AddRelations(context, stateEntry, existingVertices);
            return target;
        }

        private static string GetRelationPropertyValue(IReadOnlyList<EntityVertex> targets, bool isCollectionType)
        {
            if (isCollectionType)
                return "Collection [" + targets.Count + " elements]";

            return "[" + targets[0].KeyDescription + "]";
        }

        private static IEnumerable<object> EnumerateCurrentValue(object currentValue, bool isCollectionType)
        {
            if (currentValue == null)
                yield break;

            if (isCollectionType)
                foreach (var element in (IEnumerable)currentValue)
                    yield return element;
            else
                yield return currentValue;
        }

        private static bool IsCollectionType(Type targetEntityType)
        {
            return targetEntityType.IsArray || targetEntityType.IsGenericType;
        }

        [OnSerializing]
        internal void OnSerializingMethod(StreamingContext context)
        {
            EntityKey = null; // remove EntityKey as it isn't serializable
        }

        #region Equality members

        private bool Equals(EntityVertex other)
        {
            return OriginalHashCode == other.OriginalHashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((EntityVertex)obj);
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

        #endregion

        #region Property Helper Methods

        internal IEnumerable<NavigationProperty> GetNavigationProperties(IObjectContextAdapter context)
        {
            return context.ObjectContext.MetadataWorkspace
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .Single(p => p.FullName == EntityType.FullName)
                    .NavigationProperties;
        }

        private List<string> GetPrimaryKeyFields(MetadataWorkspace metadataWorkspace)
        {
            var metadata = metadataWorkspace
                    .GetItems<EntityType>(DataSpace.OSpace)
                    .SingleOrDefault(p => p.FullName == EntityType.FullName);

            if (metadata == null)
                throw new InvalidOperationException(String.Format("The type {0} is not known to the DbContext.", EntityType.FullName));

            return metadata.KeyMembers.Select(k => k.Name).ToList();
        }

        private List<string> GetConcurrencyFields(MetadataWorkspace metadataWorkspace)
        {
            var objType = metadataWorkspace.GetItems<EntityType>(DataSpace.OSpace).Single(p => p.FullName == EntityType.FullName);
            var cTypeName = (string)objType.GetType()
                    .GetProperty("CSpaceTypeName", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(objType, null);

            var conceptualType = metadataWorkspace.GetItems<EntityType>(DataSpace.CSpace).Single(p => p.FullName == cTypeName);
            return conceptualType.Members
                    .Where(member => member.TypeUsage.Facets.Any(facet => facet.Name == "ConcurrencyMode" && (ConcurrencyMode)facet.Value == ConcurrencyMode.Fixed))
                    .Select(member => member.Name)
                    .ToList();
        }

        #endregion
    }
}