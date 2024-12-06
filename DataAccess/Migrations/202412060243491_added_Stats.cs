namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class added_Stats : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Stats",
                c => new
                    {
                        UserId = c.Int(nullable: false),
                        Wins = c.Int(nullable: false),
                        MonstersCreated = c.Int(nullable: false),
                        AnnihilatedBabies = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .ForeignKey("dbo.Users", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Stats", "UserId", "dbo.Users");
            DropIndex("dbo.Stats", new[] { "UserId" });
            DropTable("dbo.Stats");
        }
    }
}
