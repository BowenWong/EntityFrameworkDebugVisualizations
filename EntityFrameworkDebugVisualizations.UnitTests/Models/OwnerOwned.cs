namespace EntityFramework.Debug.UnitTests.Models
{
    public class OwnerOwned : Entity
    {
        public OwnerOwned Owned { get; set; }
        public OwnerOwned Owner { get; set; }

        public string Name { get; set; }
    }
}