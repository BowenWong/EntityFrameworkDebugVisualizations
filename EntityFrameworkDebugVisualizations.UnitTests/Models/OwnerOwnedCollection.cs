using System.Collections.Generic;

namespace EntityFramework.Debug.UnitTests.Models
{
    public class OwnerOwnedCollection : Entity
    {
        public OwnerOwnedCollection Owner { get; set; }

        public List<OwnerOwnedCollection> OwnedChildren { get; set; }
    }
}