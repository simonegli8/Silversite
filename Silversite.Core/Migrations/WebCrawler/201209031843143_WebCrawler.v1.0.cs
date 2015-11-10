namespace Silversite.Migrations.WebCrawler
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WebCrawlerv10 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Silversite_Services_CompanyCategory",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 128),
                        WebAddress_Key = c.Int(),
                    })
                .PrimaryKey(t => t.Key)
                .ForeignKey("dbo.Silversite_Services_WebAddress", t => t.WebAddress_Key)
                .Index(t => t.WebAddress_Key);
            
            CreateTable(
                "dbo.Silversite_Services_WebAddress",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Title = c.String(maxLength: 64),
                        FirstName = c.String(maxLength: 128),
                        LastName = c.String(maxLength: 128),
                        Company = c.String(maxLength: 128),
                        Address = c.String(maxLength: 128),
                        Zip = c.String(maxLength: 32),
                        City = c.String(maxLength: 128),
                        Country = c.String(maxLength: 64),
                        State = c.String(maxLength: 64),
                        TimeZone = c.String(maxLength: 32),
                        Language = c.String(maxLength: 16),
                        Phone = c.String(maxLength: 128),
                        Email = c.String(maxLength: 128),
                        UserName = c.String(maxLength: 128),
                        RegistrationDate = c.DateTime(nullable: false),
                        DiskSpaceUsed = c.Long(nullable: false),
                        Providers = c.String(),
                    })
                .PrimaryKey(t => t.Key);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Silversite_Services_CompanyCategory", new[] { "WebAddress_Key" });
            DropForeignKey("dbo.Silversite_Services_CompanyCategory", "WebAddress_Key", "dbo.Silversite_Services_WebAddress");
            DropTable("dbo.Silversite_Services_WebAddress");
            DropTable("dbo.Silversite_Services_CompanyCategory");
        }
    }
}
