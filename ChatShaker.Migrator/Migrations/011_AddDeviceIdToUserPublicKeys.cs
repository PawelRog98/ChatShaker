using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202604081200)]
public class AddDeviceIdToUserPublicKeys : Migration
{
    public override void Up()
    {
        if (!Schema.Table("UserPublicKeys").Column("DeviceId").Exists())
        {
            Alter.Table("UserPublicKeys")
                .AddColumn("DeviceId").AsString(64).NotNullable().WithDefaultValue("unknown");
        }
    }

    public override void Down()
    {
        if (Schema.Table("UserPublicKeys").Column("DeviceId").Exists())
        {
            Delete.Column("DeviceId").FromTable("UserPublicKeys");
        }
    }
}
