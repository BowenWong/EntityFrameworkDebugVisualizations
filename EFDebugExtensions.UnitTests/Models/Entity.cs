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
        public Entity Child { get; set; }
    }
}