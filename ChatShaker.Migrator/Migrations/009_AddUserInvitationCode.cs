using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202602251200)]
public class AddUserInvitationCode : Migration
{
    public override void Up()
    {
        if (!Schema.Table("Users").Column("UserInvitationCode").Exists())
        {
            Alter.Table("Users")
                .AddColumn("UserInvitationCode").AsString(255).Nullable();

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_Users_UserInvitationCode")
                .OnTable("Users")
                .OnColumn("UserInvitationCode");
        }
    }

    public override void Down()
    {
        if (Schema.Table("Users").Column("UserInvitationCode").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_Users_UserInvitationCode");
            Delete.Column("UserInvitationCode").FromTable("Users");
        }
    }
}