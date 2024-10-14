namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateUserAccountRelationship : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Accounts", "UserId");
            RenameColumn(table: "dbo.Accounts", name: "AccountId", newName: "UserId");
            RenameIndex(table: "dbo.Accounts", name: "IX_AccountId", newName: "IX_UserId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Accounts", name: "IX_UserId", newName: "IX_AccountId");
            RenameColumn(table: "dbo.Accounts", name: "UserId", newName: "AccountId");
            AddColumn("dbo.Accounts", "UserId", c => c.Int(nullable: false));
        }
    }
}
