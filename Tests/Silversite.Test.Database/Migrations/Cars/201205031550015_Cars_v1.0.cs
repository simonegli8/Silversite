namespace Silversite.EntityTest.Cars
{
    using System.Data.Entity.Migrations;
    
    public partial class Cars_v10 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Silversite_Test_Database_Cars",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Model = c.String(maxLength: 64),
                    })
                .PrimaryKey(t => t.Key);
            
        }
        
        public override void Down()
        {
            DropTable("Silversite_Test_Database_Cars");
        }
    }
}
