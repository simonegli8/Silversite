namespace Silversite.Migrations {

	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;

	public sealed class SilversiteConfiguration : DbMigrationsConfiguration<Data.MigrationContext<Context>> {

		public SilversiteConfiguration() {
			AutomaticMigrationsEnabled = false;
			MigrationsNamespace = "Silversite.Migrations.Silversite";
			MigrationsDirectory = "Migrations\\Silversite";
		}

	}
}
