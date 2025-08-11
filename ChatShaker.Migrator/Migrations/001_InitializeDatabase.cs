using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Migrator.Migrations
{
    [Migration(202507181200)]
    public class InitializeDatabase : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Roles").Exists())
            {
                Create.Table("Roles")
                    .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                    .WithColumn("PublicId").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid)
                    .WithColumn("RoleName").AsString(512);
            }

            if (!Schema.Table("Users").Exists())
            {
                Create.Table("Users")
                    .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                    .WithColumn("PublicId").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid)
                    .WithColumn("PublicNick").AsString(512).NotNullable()
                    .WithColumn("FirstName").AsString(512).NotNullable()
                    .WithColumn("LastName").AsString(512).NotNullable()
                    .WithColumn("Email").AsString(512).NotNullable()
                    .WithColumn("PasswordHash").AsString(4096).NotNullable()
                    .WithColumn("DateOfBirth").AsDateTime().NotNullable()
                    .WithColumn("IsEmailConfirmed").AsBoolean().NotNullable()
                    .WithColumn("LastActivityDateTime").AsDateTime2().Nullable()
                    .WithColumn("AccountInfo").AsString(4096).Nullable()
                    .WithColumn("RoleId").AsInt64().Nullable()
                    .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable()
                    .WithColumn("ModifiedAtUtc").AsDateTime2().Nullable();

                Create.ForeignKey("FK_Users_RoleId")
                    .FromTable("Users").ForeignColumn("RoleId")
                    .ToTable("Roles").PrimaryColumn("Id")
                    .OnDelete(System.Data.Rule.None);
            }
        }
        public override void Down()
        {

            if (Schema.Table("Users").Exists())
            {
                Delete.ForeignKey("FK_Users_RoleId").OnTable("Users");
                Delete.Table("Users");
            }

            if (Schema.Table("Roles").Exists())
            {
                Delete.Table("Roles");
            }
        }
    }
}
