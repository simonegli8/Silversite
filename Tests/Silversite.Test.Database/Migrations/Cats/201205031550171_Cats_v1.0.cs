namespace Silversite.EntityTest.Cats
{
    using System.Data.Entity.Migrations;
    
    public partial class Cats_v10 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "Silversite_Test_Database_Cats",
                c => new
                    {
                        Key = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 128),
                        Color = c.String(),
                    })
                .PrimaryKey(t => new { t.Key, t.Name });
            
        }
        
        public override void Down()
        {
            DropTable("Silversite_Test_Database_Cats");
        }
    }
}
