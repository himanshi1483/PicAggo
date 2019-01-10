namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class tableupdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.EventGroupMappings", "GroupId", c => c.Int(nullable: false));
            AddColumn("dbo.UserGroupMappings", "IsInviteAccepted", c => c.Boolean(nullable: false));
            DropColumn("dbo.EventGroupMappings", "UserGroupId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EventGroupMappings", "UserGroupId", c => c.Int(nullable: false));
            DropColumn("dbo.UserGroupMappings", "IsInviteAccepted");
            DropColumn("dbo.EventGroupMappings", "GroupId");
        }
    }
}
