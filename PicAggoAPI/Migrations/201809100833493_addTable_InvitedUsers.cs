namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addTable_InvitedUsers : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.InvitedUsers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PhoneNumber = c.String(),
                        IsInvitationAccepted = c.Boolean(nullable: false),
                        EventId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.InvitedUsers");
        }
    }
}
