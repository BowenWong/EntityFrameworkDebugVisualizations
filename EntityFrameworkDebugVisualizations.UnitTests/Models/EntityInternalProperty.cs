namespace EntityFramework.Debug.UnitTests.Models
{
    // http://blog.oneunicorn.com/2012/03/26/code-first-data-annotations-on-non-public-properties/
    public class EntityInternalProperty : Entity
    {
        internal string InternalProperty { get; set; }
    }
}