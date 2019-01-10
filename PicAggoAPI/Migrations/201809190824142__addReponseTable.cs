namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _addReponseTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.StoredResponses",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        access_token = c.String(),
                        token_type = c.String(),
                        expires_in = c.Long(),
                        refresh_token = c.String(),
                        Issued = c.String(),
                        UserID = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.AspNetUsers", "AppParentFolderId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "AppParentFolderId");
            DropTable("dbo.StoredResponses");
        }
    }
}
