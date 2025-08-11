using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Migrator.Migrations
{
    [Migration(202507181305)]
    public class AddTokensTable : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Tokens").Exists())
            {
                Create.Table("Tokens")
                    .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                    .WithColumn("PublicId").AsGuid().NotNullable().WithDefault(SystemMethods.NewGuid)
                    .WithColumn("TokenData").AsString(512).NotNullable()
                    .WithColumn("ExpireDateTime").AsDateTime2().Nullable()
                    .WithColumn("TokenTypeValue").AsString(64).NotNullable()
                    .WithColumn("UserId").AsInt64();

                Create.ForeignKey("FK_Tokens_UserId")
                    .FromTable("Tokens").ForeignColumn("UserId")
                    .ToTable("Users").PrimaryColumn("Id")
                    .OnDelete(System.Data.Rule.None);
            }
        }

        public override void Down()
        {
            if (Schema.Table("Tokens").Exists())
            {
                Delete.ForeignKey("FK_Tokens_UserId").OnTable("Tokens");
                Delete.Table("Tokens");
            }
        }
    }
}
