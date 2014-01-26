using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFramework.Debug.UnitTests.Models
{
    public class GuidEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
    }
}