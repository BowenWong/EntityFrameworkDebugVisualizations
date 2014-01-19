using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EntityFramework.Debug.UnitTests.Models
{
    public class Entity
    {
        [Key]
        public int Id { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
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
}