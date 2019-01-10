namespace PicAggoAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _changes1 : DbMigration
    {
        public override void Up()
        {
           // RenameTable(name: "dbo.GroupsMasters", newName: "GroupMasters");
        }
        
        public override void Down()
        {
           // RenameTable(name: "dbo.GroupMasters", newName: "GroupsMasters");
        }
    }
}
