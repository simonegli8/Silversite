namespace Silversite.EntityTest.Migrations {

    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class MigrationConfiguration : DbMigrationsConfiguration<Migrations.PersonContext> {
    
		public MigrationConfiguration(): base() {
            AutomaticMigrationsEnabled = false;
			//MigrationsAssembly = GetType().Assembly;
			MigrationsNamespace = "Silversite.EntityTest.Migrations";
        }
    }
}
