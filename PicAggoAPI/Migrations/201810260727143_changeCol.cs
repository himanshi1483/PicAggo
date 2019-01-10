namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class changeCol : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Events", "StartTIme", c => c.DateTime());
            AlterColumn("dbo.Events", "EndTime", c => c.DateTime());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Events", "EndTime", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Events", "StartTIme", c => c.DateTime(nullable: false));
        }
    }
}
