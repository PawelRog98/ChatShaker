using System.Data;
using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202602191100)]
public class AddMessageRelations : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Messages").Column("RelatedToId").Exists())
        {
            Alter.Table("Messages")
                .AddColumn("RelatedToId").AsInt64().Nullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Messages_RelatedToId")
                .FromTable("Messages").ForeignColumn("RelatedToId")
                .ToTable("Messages").PrimaryColumn("Id")
                .OnDelete(Rule.None);
        }
    }

    public override void Down()
    {
        if (Schema.Table("Messages").Column("RelatedToId").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Messages_RelatedToId").OnTable("Messages");
            Delete.Column("RelatedToId").FromTable("Messages");
        }
    }
}