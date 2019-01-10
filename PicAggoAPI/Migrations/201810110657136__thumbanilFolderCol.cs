namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _thumbanilFolderCol : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "ThumbFolderId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "ThumbFolderId");
        }
    }
}
