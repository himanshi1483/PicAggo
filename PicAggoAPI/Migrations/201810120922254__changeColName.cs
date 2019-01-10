namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _changeColName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserGroupMappings", "UserName", c => c.String());
            DropColumn("dbo.UserGroupMappings", "UserId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserGroupMappings", "UserId", c => c.String());
            DropColumn("dbo.UserGroupMappings", "UserName");
        }
    }
}
