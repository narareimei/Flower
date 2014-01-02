

namespace FlowerEngine.Model
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Common;

    
    public partial class FlowerEntities : DbContext
    {
        public FlowerEntities(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }
    }

    public partial class FlowerEntitiesEx : FlowerEntities
    {
        public FlowerEntitiesEx(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }


}

