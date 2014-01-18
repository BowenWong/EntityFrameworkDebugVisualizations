using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using EntityFramework.Debug.DebugVisualization.Graph;

namespace EntityFramework.Debug
{
    public static class DebugExtensions
    {
        public static string DumpTrackedEntities(this DbContext context)
        {
            return ((IObjectContextAdapter)context).DumpTrackedEntities();
        }

        public static string DumpTrackedEntities(this IObjectContextAdapter context)
        {
            var trackedEntities = context.ObjectContext.ObjectStateManager
                    .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged)
#warning what about entry.IsRelationship? are those deleted references?
                    .Where(entry => !entry.IsRelationship && entry.State != EntityState.Detached && entry.Entity != null)
                    .SelectMany(entry => (entry.State != EntityState.Deleted ? entry.CurrentValues : (CurrentValueRecord) entry.OriginalValues)
                                        .DataRecordInfo
                                        .FieldMetadata
                                        .Select((field, index) => new
                                        {
                                            entry.State,
                                            EntityType = entry.Entity.GetType(),
                                            HashCode = entry.Entity.GetHashCode(),
                                            PropertyName = entry.CurrentValues.DataRecordInfo.FieldMetadata[index].FieldType.Name,
                                            CurrentValue = entry.State != EntityState.Deleted ? entry.CurrentValues.GetValue(index) : null,
                                            OriginalValue = entry.State != EntityState.Added ? entry.OriginalValues.GetValue(index) : null,
#warning this contains information about the relations => do I have something like original and current? what about the name?
                                            Rels = entry.RelationshipManager.GetAllRelatedEnds()
                                        }))
                    .OrderBy(e => e.State).ThenBy(e => e.EntityType.Name)
                    .ToList();

            var builder = new StringBuilder();

            EntityState? previousState = null;
            int previousHashCode = 0;
            foreach (var entity in trackedEntities)
            {
                if (entity.State != previousState)
                {
                    if (builder.Length > 0)
                        builder.AppendLine();

                    builder.AppendLine(entity.State.ToString());
                    builder.AppendLine("----------");
                }
                previousState = entity.State;

                if (entity.HashCode != previousHashCode)
                {
                    if (previousHashCode > 0)
                        builder.AppendLine();
                    builder.AppendFormat("{0} (# {1})", entity.EntityType.Name, entity.HashCode).AppendLine();
                }

                previousHashCode = entity.HashCode;

                switch (entity.State)
                {
                    case EntityState.Added:
                        builder.AppendFormat("  {0}: '{1}'", entity.PropertyName, TrimToMaxLength(entity.CurrentValue.ToString())).AppendLine();
                        break;
                    case EntityState.Deleted:
                        builder.AppendFormat("  {0}: '{1}'", entity.PropertyName, TrimToMaxLength(entity.OriginalValue.ToString())).AppendLine();
                        break;
                    default:
                        builder.AppendFormat("  {0}: changed from '{1}' to '{2}'", entity.PropertyName, TrimToMaxLength(entity.OriginalValue.ToString()),
                                             TrimToMaxLength(entity.CurrentValue.ToString())).AppendLine();
                        break;
                }
            }
            return builder.ToString();
        }

        public static List<EntityVertex> GetEntityVertices(this DbContext context)
        {
            return ((IObjectContextAdapter)context).GetEntityVertices();
        }

        public static List<EntityVertex> GetEntityVertices(this IObjectContextAdapter context)
        {
            return context.ObjectContext
                    .ObjectStateManager
                    .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified | EntityState.Unchanged)
                    .Where(entry => entry.State != EntityState.Detached && entry.Entity != null)
                    .Select(entry => CreateEntityVertex(entry))
                    .Where(e => e != null)
                    .OrderBy(e => e.State).ThenBy(e => e.TypeName)
                    .ToList();
        }

        private static EntityVertex CreateEntityVertex(ObjectStateEntry entry)
        {
#warning what about entry.IsRelationship? are those deleted references?
            if (entry.IsRelationship)
                return null;

            var entityVertex = new EntityVertex
            {
                State = entry.State,
                FullTypeName = entry.Entity.GetType().FullName,
                TypeName = entry.Entity.GetType().Name,
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

            //#warning this contains information about the relations => do I have something like original and current? what about the name?
            //                                            Rels = entry.RelationshipManager.GetAllRelatedEnds()

            return entityVertex;
        }

        private static string TrimToMaxLength(string toTrim)
        {
            const int maxLength = 150;
            if (toTrim.Length <= maxLength)
                return toTrim;

            return toTrim.Substring(0, maxLength) + " [..]";
        }
    }
}