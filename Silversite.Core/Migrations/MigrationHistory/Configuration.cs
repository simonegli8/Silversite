namespace Silversite.Migrations {

	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;

	internal sealed class MigrationHistoryConfiguration: DbMigrationsConfiguration<Data.MigrationContext<Data.MigrationContext>> {

		public MigrationHistoryConfiguration() {
			AutomaticMigrationsEnabled = false;
			MigrationsNamespace = "Silversite.Migrations.MigrationHistory";
			MigrationsDirectory = "Migrations\\MigrationHistory";
		}

	}
}
