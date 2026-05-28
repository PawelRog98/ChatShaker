using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202604081300)]
public class UpdateChatRoomKeyBlobsPrimaryKey : Migration
{
    public override void Up()
    {
        if (Schema.Table("ChatRoomKeyBlobs").Exists())
        {
            Delete.PrimaryKey("PK_ChatRoomKeyBlobs").FromTable("ChatRoomKeyBlobs");
            
            Create.PrimaryKey("PK_ChatRoomKeyBlobs")
                .OnTable("ChatRoomKeyBlobs")
                .Columns("ChatRoomId", "UserId", "DeviceId");
        }
    }

    public override void Down()
    {
        if (Schema.Table("ChatRoomKeyBlobs").Exists())
        {
            Delete.PrimaryKey("PK_ChatRoomKeyBlobs").FromTable("ChatRoomKeyBlobs");

            Create.PrimaryKey("PK_ChatRoomKeyBlobs")
                .OnTable("ChatRoomKeyBlobs")
                .Columns("ChatRoomId", "UserId");
        }
    }
}
