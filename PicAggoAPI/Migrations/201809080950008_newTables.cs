namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newTables : DbMigration
    {
        public override void Up()
        {
            //RenameTable(name: "dbo.LogMetadatas", newName: "AuditLogs");
            //RenameTable(name: "dbo.MemberGroups", newName: "GroupsMasters");
            CreateTable(
                "dbo.EventDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        SessionActivity = c.String(),
                        ActivityTime = c.DateTime(nullable: false),
                        ActivityBy = c.String(),
                        PicturesUploaded = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EventGroupMappings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventId = c.Int(nullable: false),
                        UserGroupId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventName = c.String(),
                        Description = c.String(),
                        Duration = c.String(),
                        StartTIme = c.DateTime(nullable: false),
                        EndTime = c.DateTime(nullable: false),
                        CreatedBy = c.String(),
                        AlbumLocation = c.String(),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.UserGroupMappings",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    UserId = c.String(),
                    GroupId = c.Int(nullable: false),
                    ApplicationUser_Id = c.String(maxLength: 128),
                    GroupsMaster_Id = c.Int(),
                })
                .PrimaryKey(t => t.Id);
              
            
        }
        
        public override void Down()
        {
          
            DropTable("dbo.UserGroupMappings");
            DropTable("dbo.Events");
            DropTable("dbo.EventGroupMappings");
            DropTable("dbo.EventDetails");
            //RenameTable(name: "dbo.GroupsMasters", newName: "MemberGroups");
            //RenameTable(name: "dbo.AuditLogs", newName: "LogMetadatas");
        }
    }
}
