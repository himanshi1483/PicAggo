namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class removeTable : DbMigration
    {
        public override void Up()
        {
          //  DropTable("dbo.AuditLogs");
        }
        
        public override void Down()
        {
            //CreateTable(
            //    "dbo.AuditLogs",
            //    c => new
            //        {
            //            Id = c.Int(nullable: false, identity: true),
            //            RequestContentType = c.String(),
            //            RequestUri = c.String(),
            //            RequestMethod = c.String(),
            //            RequestTimestamp = c.DateTime(),
            //            ResponseContentType = c.String(),
            //            ResponseStatusCode = c.String(),
            //            ResponseTimestamp = c.DateTime(),
            //        })
            //    .PrimaryKey(t => t.Id);
            
        }
    }
}
