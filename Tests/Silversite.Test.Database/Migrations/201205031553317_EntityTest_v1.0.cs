namespace Silversite.EntityTest.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class EntityTest_v10 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Silversite_Test_Database_Persons",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 128),
                        City = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Key);
            
        }
        
        public override void Down()
        {
            DropTable("Silversite_Test_Database_Persons");
        }
    }
}
