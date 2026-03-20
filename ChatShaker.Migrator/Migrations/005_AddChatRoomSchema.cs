using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202510271100)]
public class AddChatRoomSchema : Migration
{
    public override void Up()
    {
        if (!Schema.Table("ChatRooms").Exists())
        {
            Create.Table("ChatRooms")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("HostId").AsInt64().NotNullable()
                .WithColumn("Name").AsString(64).NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_ChatRooms_HostId")
                .FromTable("ChatRooms").ForeignColumn("HostId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_ChatRooms_PublicId")
                .OnTable("ChatRooms").OnColumn("PublicId").Ascending()
                .WithOptions().Unique();
        }

        if (!Schema.Table("ChatRoomMemberships").Exists())
        {
            Create.Table("ChatRoomMemberships")
                .WithColumn("ChatRoomId").AsInt64().NotNullable()
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("AddedById").AsInt64().NotNullable();

            Create.PrimaryKey("PK_ChatRoomMemberships")
                .OnTable("ChatRoomMemberships")
                .Columns("ChatRoomId", "UserId");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_ChatRoomMemberships_ChatRoomId")
                .FromTable("ChatRoomMemberships").ForeignColumn("ChatRoomId")
                .ToTable("ChatRooms").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_ChatRoomMemberships_UserId")
                .FromTable("ChatRoomMemberships").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_ChatRoomMemberships_AddedById")
                .FromTable("ChatRoomMemberships").ForeignColumn("AddedById")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);
        }

        if (!Schema.Table("Messages").Exists())
        {
            Create.Table("Messages")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("ChatRoomId").AsInt64().NotNullable()
                .WithColumn("SenderId").AsInt64().NotNullable()
                .WithColumn("CipherText").AsString(int.MaxValue).NotNullable()
                .WithColumn("Nonce").AsString(128).NotNullable()
                .WithColumn("SentAtUtc").AsDateTime2().NotNullable()
                .WithColumn("ClientMessageId").AsGuid().Nullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Messages_ChatRoomId")
                .FromTable("Messages").ForeignColumn("ChatRoomId")
                .ToTable("ChatRooms").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Messages_SenderId")
                .FromTable("Messages").ForeignColumn("SenderId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_Messages_PublicId")
                .OnTable("Messages").OnColumn("PublicId").Ascending()
                .WithOptions().Unique();
        }

        if (!Schema.Table("ChatRoomKeyBlobs").Exists())
        {
            Create.Table("ChatRoomKeyBlobs")
                .WithColumn("ChatRoomId").AsInt64().NotNullable()
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("EncryptedRoomKey").AsString(int.MaxValue).NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable()
                .WithColumn("Version").AsInt64().NotNullable()
                .WithColumn("DeviceId").AsString(64).NotNullable();

            Create.PrimaryKey("PK_ChatRoomKeyBlobs")
                .OnTable("ChatRoomKeyBlobs")
                .Columns("ChatRoomId", "UserId");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_ChatRoomKeyBlobs_ChatRoomId")
                .FromTable("ChatRoomKeyBlobs").ForeignColumn("ChatRoomId")
                .ToTable("ChatRooms").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_ChatRoomKeyBlobs_UserId")
                .FromTable("ChatRoomKeyBlobs").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);
        }
    }

    public override void Down()
    {
        if (Schema.Table("ChatRoomKeyBlobs").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_ChatRoomKeyBlobs_ChatRoomId").OnTable("ChatRoomKeyBlobs");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_ChatRoomKeyBlobs_UserId").OnTable("ChatRoomKeyBlobs");

            Delete.Table("ChatRoomKeyBlobs");
        }

        if (Schema.Table("Messages").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_Messages_PublicId").OnTable("Messages");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Messages_ChatRoomId").OnTable("Messages");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Messages_SenderId").OnTable("Messages");

            Delete.Table("Messages");
        }

        if (Schema.Table("ChatRoomMemberships").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_ChatRoomMemberships_ChatRoomId").OnTable("ChatRoomMemberships");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_ChatRoomMemberships_UserId").OnTable("ChatRoomMemberships");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_ChatRoomMemberships_AddedById").OnTable("ChatRoomMemberships");

            Delete.Table("ChatRoomMemberships");
        }

        if (Schema.Table("ChatRooms").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_ChatRooms_PublicId").OnTable("CHatRooms");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_ChatRooms_HostId").OnTable("ChatRooms");

            Delete.Table("ChatRooms");
        }
    }
}
