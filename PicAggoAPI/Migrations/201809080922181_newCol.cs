namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class newCol : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AspNetUsers", "DefaultStorage", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AspNetUsers", "DefaultStorage");
        }
    }
}
