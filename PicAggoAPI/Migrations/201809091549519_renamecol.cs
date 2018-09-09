namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renamecol : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.UserGroupMappings", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.UserGroupMappings", "GroupsMaster_Id", "dbo.GroupsMasters");
            DropIndex("dbo.UserGroupMappings", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.UserGroupMappings", new[] { "GroupsMaster_Id" });
          //  AlterColumn("dbo.AuditLogs", "ResponseStatusCode", c => c.String());
            DropColumn("dbo.UserGroupMappings", "ApplicationUser_Id");
            DropColumn("dbo.UserGroupMappings", "GroupsMaster_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserGroupMappings", "GroupsMaster_Id", c => c.Int());
            AddColumn("dbo.UserGroupMappings", "ApplicationUser_Id", c => c.String(maxLength: 128));
        //    AlterColumn("dbo.AuditLogs", "ResponseStatusCode", c => c.Int(nullable: false));
            CreateIndex("dbo.UserGroupMappings", "GroupsMaster_Id");
            CreateIndex("dbo.UserGroupMappings", "ApplicationUser_Id");
            AddForeignKey("dbo.UserGroupMappings", "GroupsMaster_Id", "dbo.GroupsMasters", "Id");
            AddForeignKey("dbo.UserGroupMappings", "ApplicationUser_Id", "dbo.AspNetUsers", "Id");
        }
    }
}
