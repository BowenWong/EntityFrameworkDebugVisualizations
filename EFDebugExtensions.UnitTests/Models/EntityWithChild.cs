using System.Collections.Generic;

namespace EntityFramework.Debug.UnitTests.Models
{
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
}