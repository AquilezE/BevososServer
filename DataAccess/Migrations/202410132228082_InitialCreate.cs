namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Accounts",
                c => new
                    {
                        AccountId = c.Int(nullable: false),
                        Email = c.String(nullable: false, maxLength: 100),
                        PasswordHash = c.String(nullable: false),
                        UserId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.AccountId)
                .ForeignKey("dbo.Users", t => t.AccountId, cascadeDelete: true)
                .Index(t => t.AccountId)
                .Index(t => t.Email, unique: true, name: "IX_AccountEmail");
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        Username = c.String(nullable: false, maxLength: 50),
                        ProfilePictureId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.UserId)
                .Index(t => t.Username, unique: true, name: "IX_UserUsername");
            
            CreateTable(
                "dbo.Tokens",
                c => new
                    {
                        TokenId = c.Int(nullable: false, identity: true),
                        Email = c.String(nullable: false),
                        TokenValue = c.String(nullable: false),
                        ExpiryDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.TokenId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Accounts", "AccountId", "dbo.Users");
            DropIndex("dbo.Users", "IX_UserUsername");
            DropIndex("dbo.Accounts", "IX_AccountEmail");
            DropIndex("dbo.Accounts", new[] { "AccountId" });
            DropTable("dbo.Tokens");
            DropTable("dbo.Users");
            DropTable("dbo.Accounts");
        }
    }
}
