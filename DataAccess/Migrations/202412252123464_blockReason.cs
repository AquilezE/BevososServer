namespace DataAccess.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class blockReason : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Blockeds", "Reason", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Blockeds", "Reason");
        }
    }
}
