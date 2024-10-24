namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddedFriendsRequestFriendshipBlocked : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Blockeds",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlockerId = c.Int(nullable: false),
                        BlockeeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.BlockeeId)
                .ForeignKey("dbo.Users", t => t.BlockerId)
                .Index(t => t.BlockerId)
                .Index(t => t.BlockeeId);
            
            CreateTable(
                "dbo.FriendRequests",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RequesterId = c.Int(nullable: false),
                        RequesteeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.RequesteeId)
                .ForeignKey("dbo.Users", t => t.RequesterId)
                .Index(t => t.RequesterId)
                .Index(t => t.RequesteeId);
            
            CreateTable(
                "dbo.Friendships",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        User1Id = c.Int(nullable: false),
                        User2Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User1Id)
                .ForeignKey("dbo.Users", t => t.User2Id)
                .Index(t => t.User1Id)
                .Index(t => t.User2Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Friendships", "User2Id", "dbo.Users");
            DropForeignKey("dbo.Friendships", "User1Id", "dbo.Users");
            DropForeignKey("dbo.FriendRequests", "RequesterId", "dbo.Users");
            DropForeignKey("dbo.FriendRequests", "RequesteeId", "dbo.Users");
            DropForeignKey("dbo.Blockeds", "BlockerId", "dbo.Users");
            DropForeignKey("dbo.Blockeds", "BlockeeId", "dbo.Users");
            DropIndex("dbo.Friendships", new[] { "User2Id" });
            DropIndex("dbo.Friendships", new[] { "User1Id" });
            DropIndex("dbo.FriendRequests", new[] { "RequesteeId" });
            DropIndex("dbo.FriendRequests", new[] { "RequesterId" });
            DropIndex("dbo.Blockeds", new[] { "BlockeeId" });
            DropIndex("dbo.Blockeds", new[] { "BlockerId" });
            DropTable("dbo.Friendships");
            DropTable("dbo.FriendRequests");
            DropTable("dbo.Blockeds");
        }
    }
}
