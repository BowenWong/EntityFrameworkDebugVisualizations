using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Debug.UnitTests.Models
{
    public class Entity
    {
        [Key]
        public int Id { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }

    public class MultiKeyEntity
    {
        [Key]
        [Column(Order = 1)]
        public string KeyPart1 { get; set; }

        [Key]
        [Column(Order = 2)]
        public string KeyPart2 { get; set; }

        public string Name { get; set; }
    }

    public class EntityWithChild : Entity
    {
        public string Name { get; set; }
        public Entity FavoriteChild { get; set; }
        public List<EntityWithChild> Children { get; set; }

        public EntityWithChild()
        {
            Children = new List<EntityWithChild>();
        }
    }

    public class OwnerOwned : Entity
    {
        public OwnerOwned Owned { get; set; }
        public OwnerOwned Owner { get; set; }
    }

    public class OwnerOwnedCollection : Entity
    {
        public OwnerOwnedCollection Owner { get; set; }

        public List<OwnerOwnedCollection> OwnedChildren { get; set; }
    }
}