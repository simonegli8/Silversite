namespace Silversite.EntityTest.Cats {

    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<CatContext> {

        public Configuration() {
            AutomaticMigrationsEnabled = false;
			MigrationsNamespace = "Silversite.EntityTest.Cats";
			MigrationsDirectory = "Migrations\\Cats";
        }

    }
}
