namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _addColRemoveCol : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Events", "ThumbLocation", c => c.String());
            DropColumn("dbo.AspNetUsers", "ThumbFolderId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.AspNetUsers", "ThumbFolderId", c => c.String());
            DropColumn("dbo.Events", "ThumbLocation");
        }
    }
}
