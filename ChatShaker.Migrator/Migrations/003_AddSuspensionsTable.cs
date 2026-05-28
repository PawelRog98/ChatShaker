using FluentMigrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Migrator.Migrations
{
    [Migration(202507181400)]
    public class AddSuspensionsTable : Migration
    {
        public override void Up()
        {
            if (!Schema.Table("Suspensions").Exists())
            {
                Create.Table("Suspensions")
                    .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                    .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                    .WithColumn("StartDate").AsDateTime2().NotNullable()
                    .WithColumn("EndDate").AsDateTime2().Nullable()
                    .WithColumn("Reason").AsString(512).NotNullable()
                    .WithColumn("Status").AsString(64).NotNullable()
                    .WithColumn("UserId").AsInt64().NotNullable()
                    .WithColumn("SuspendedById").AsInt64().NotNullable();

                IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                    .Create.ForeignKey("FK_Suspensions_UserId")
                    .FromTable("Suspensions").ForeignColumn("UserId")
                    .ToTable("Users").PrimaryColumn("Id")
                    .OnDelete(System.Data.Rule.None);
                
                IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                    .Create.ForeignKey("FK_Suspensions_SuspendedById")
                    .FromTable("Suspensions").ForeignColumn("SuspendedById")
                    .ToTable("Users").PrimaryColumn("Id")
                    .OnDelete(System.Data.Rule.None);
            }
        }

        public override void Down()
        {
            if (Schema.Table("Suspensions").Exists())
            {
                IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                    .Delete.ForeignKey("FK_Suspensions_SuspendedById").OnTable("Suspensions");

                IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                    .Delete.ForeignKey("FK_Suspensions_UserId").OnTable("Suspensions");
                    
                Delete.Table("Suspensions");
            }
        }
    }
}
