namespace Silversite.Migrations {

	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;

	public sealed class WebCrawlerConfiguration : DbMigrationsConfiguration<Data.MigrationContext<Services.WebCrawlerContext>> {

		public WebCrawlerConfiguration() {
			AutomaticMigrationsEnabled = false;
			MigrationsNamespace = "Silversite.Migrations.WebCrawler";
			MigrationsDirectory = "Migrations\\WebCrawler";
		}

	}
}
