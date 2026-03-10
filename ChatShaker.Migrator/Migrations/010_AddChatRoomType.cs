using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202602271300)]
public class AddChatRoomType : Migration
{
    public override void Up()
    {
        if (!Schema.Table("ChatRooms").Column("Type").Exists())
        {
            Alter.Table("ChatRooms")
                .AddColumn("Type").AsString(32).NotNullable()
                .AddColumn("IsInitialized").AsBoolean().NotNullable();
        }
    }

    public override void Down()
    {
        if (Schema.Table("ChatRooms").Column("Type").Exists())
        {
            Delete.Column("Type").FromTable("ChatRooms");
            Delete.Column("IsInitialized").FromTable("ChatRooms");
        }
    }
}