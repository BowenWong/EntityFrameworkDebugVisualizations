using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Debug.UnitTests.Models
{
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
}