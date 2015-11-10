namespace Silversite.EntityTest {

    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class Configuration : DbMigrationsConfiguration<PersonContext> {
    
		public Configuration(): base() {
            AutomaticMigrationsEnabled = false;
			//MigrationsAssembly = GetType().Assembly;
			MigrationsNamespace = "Silversite.EntityTest.Migrations";
        }
    }
}
