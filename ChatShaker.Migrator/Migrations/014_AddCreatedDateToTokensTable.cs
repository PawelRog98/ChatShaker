using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202605171200)]
public class AddCreatedDateToTokensTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Tokens").Column("CreatedDateUtc").Exists())
        {
            Alter.Table("Tokens").AddColumn("CreatedDateUtc")
                .AsDateTime2().NotNullable().WithDefaultValue(DateTime.UtcNow.AddDays(-3));
        }
    }

    public override void Down()
    {
        if (Schema.Table("Tokens").Column("CreatedDateUtc").Exists())
        {
            Delete.Column("CreatedDateUtc").FromTable("Tokens");
        }
    }
}