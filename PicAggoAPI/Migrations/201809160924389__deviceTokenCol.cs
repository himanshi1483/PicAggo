namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _deviceTokenCol : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "DeviceToken", c => c.String());
        }

        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "DeviceToken");
        }
    }
}
