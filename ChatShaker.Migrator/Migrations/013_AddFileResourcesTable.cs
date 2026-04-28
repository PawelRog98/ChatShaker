using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202604281200)]
public class AddFileResourcesTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("FileResources").Exists())
        {
            Create.Table("FileResources")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("FileName").AsString(255).NotNullable()
                .WithColumn("Description").AsString(1024).Nullable()
                .WithColumn("ContentType").AsString(128).Nullable()
                .WithColumn("ChatRoomId").AsInt64().Nullable()
                .WithColumn("Size").AsInt64().NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable()
                .WithColumn("Type").AsString(64).NotNullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_FileResources_ChatRoomId")
                .FromTable("FileResources").ForeignColumn("ChatRoomId")
                .ToTable("ChatRooms").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_FileResources_PublicId")
                .OnTable("FileResources").OnColumn("PublicId").Ascending()
                .WithOptions().Unique();
        }

        if (!Schema.Table("Messages").Column("MessageType").Exists())
        {
            Alter.Table("Messages")
                .AddColumn("MessageType").AsByte().NotNullable().WithDefaultValue(0);
        }
    }

    public override void Down()
    {
        if (Schema.Table("Messages").Column("MessageType").Exists())
        {
            Delete.Column("MessageType").FromTable("Messages");
        }

        if (Schema.Table("FileResources").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_FileResources_PublicId").OnTable("FileResources");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_FileResources_ChatRoomId").OnTable("FileResources");

            Delete.Table("FileResources");
        }
    }
}
