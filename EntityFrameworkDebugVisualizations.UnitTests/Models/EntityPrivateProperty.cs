using System.Data.Entity.ModelConfiguration;

namespace EntityFramework.Debug.UnitTests.Models
{
    // http://romiller.com/2012/10/01/mapping-to-private-properties-with-code-first/
    public class EntityPrivateProperty : Entity
    {
        private string PrivateProperty { get; set; }

        public class EntityPrivatePropertyConfig : EntityTypeConfiguration<EntityPrivateProperty>
        {
            public EntityPrivatePropertyConfig()
            {
                Property(p => p.PrivateProperty);
            }
        }
    }
}
