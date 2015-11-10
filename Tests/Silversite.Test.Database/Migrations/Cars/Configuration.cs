namespace Silversite.EntityTest.Cars {

    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<CarContext> {

        public Configuration() {
            AutomaticMigrationsEnabled = false;
			MigrationsNamespace = "Silversite.EntityTest.Cars";
			MigrationsDirectory = "Migrations\\Cars";
        }

    }
}
