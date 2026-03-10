using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202510270900)]
public class AddUserKeysTable : Migration
{
    public override void Up()
    {
        if (!Schema.Table("UserPublicKeys").Exists()) 
        {
            Create.Table("UserPublicKeys")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("UserId").AsInt64().NotNullable()
                .WithColumn("PublicKey").AsString(512).NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_UserPublicKeys_UserId")
                .FromTable("UserPublicKeys").ForeignColumn("UserId")
                .ToTable("Users").PrimaryColumn("Id")
                .OnDelete(System.Data.Rule.None);
        }

    }
    public override void Down()
    {
        if(Schema.Table("UserPublicKeys").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_UserPublicKeys_UserId").OnTable("UserPublicKeys");

            Delete.Table("UserPublicKeys");
        }
    }
}
