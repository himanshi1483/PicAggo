namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class renamecol1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LogMetadatas",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RequestContentType = c.String(),
                        RequestUri = c.String(),
                        RequestMethod = c.String(),
                        RequestTimestamp = c.DateTime(),
                        ResponseContentType = c.String(),
                        ResponseStatusCode = c.String(),
                        ResponseTimestamp = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.LogMetadatas");
        }
    }
}
