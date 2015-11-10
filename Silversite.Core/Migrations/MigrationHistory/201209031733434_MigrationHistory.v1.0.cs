namespace Silversite.Migrations.MigrationHistory
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MigrationHistoryv10 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Silversite_Data_ContextVersion",
                c => new
                    {
                        Context = c.String(nullable: false, maxLength: 255),
                        Migrations = c.Int(nullable: false),
                        ModelHash = c.Int(nullable: false),
                        FrameworkVersion = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.Context);
            
			/*
            CreateTable(
                "dbo.__MigrationHistory",
                c => new
                    {
                        MigrationId = c.String(nullable: false, maxLength: 255),
                        Model = c.Binary(nullable: false),
                        ProductVersion = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.MigrationId);
            */

            CreateTable(
                "dbo.Silversite_Data_ContextMigrationHistory",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Context = c.String(maxLength: 255),
                        MigrationId = c.String(maxLength: 255),
                        Model = c.Binary(nullable: false),
                        ProductVersion = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.Key);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Silversite_Data_ContextMigrationHistory");
            DropTable("dbo.__MigrationHistory");
            DropTable("dbo.Silversite_Data_ContextVersion");
        }
    }
}
