using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202510281000)]
public class AddMessageStatuses : Migration
{
    public override void Up()
    {
        if (!Schema.Table("MessageStatuses").Exists())
        {
            Create.Table("MessageStatuses")
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("MessageId").AsInt64().NotNullable()
                .WithColumn("Status").AsByte().NotNullable()
                .WithColumn("UpdateAtUtc").AsDateTime2().NotNullable();

            Create.PrimaryKey("PK_MessageStatuses")
                .OnTable("MessageStatuses")
                .Columns("MessageId", "UserId");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_MessageStatuses_UserId")
                .FromTable("MessageStatuses").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_MessageStatuses_MessageId")
                .FromTable("MessageStatuses").ForeignColumn("MessageId")
                .ToTable("Messages").PrimaryColumn("Id")
                .OnDeleteOrUpdate(System.Data.Rule.None);
        }
    }

    public override void Down()
    {
        if (Schema.Table("MessageStatuses").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_MessageStatuses_MessageId").OnTable("MessageStatuses");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_MessageStatuses_UserId").OnTable("MessageStatuses");

            Delete.Table("MessageStatuses");
        }
    }
}
