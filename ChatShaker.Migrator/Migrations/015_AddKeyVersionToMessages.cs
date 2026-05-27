using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202605251200)]
public class AddKeyVersionToMessages : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Messages").Column("KeyVersion").Exists())
        {
            Alter.Table("Messages")
                .AddColumn("KeyVersion").AsInt64().NotNullable().WithDefaultValue(1);
        }
    }

    public override void Down()
    {
        if (Schema.Table("Messages").Column("KeyVersion").Exists())
        {
            Delete.Column("KeyVersion").FromTable("Messages");
        }
    }
}
