namespace Silversite.EntityTest.Cats {

    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class MigrationConfiguration : DbMigrationsConfiguration<Migrations.CatContext> {

        public MigrationConfiguration() {
            AutomaticMigrationsEnabled = false;
			MigrationsNamespace = "Silversite.EntityTest.Cats";
			MigrationsDirectory = "Migrations\\Cats";
        }

    }
}
