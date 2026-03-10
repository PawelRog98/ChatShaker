using FluentMigrator;

namespace ChatShaker.Migrator.Migrations;

[Migration(202602251100)]
public class AddFriendshipTables : Migration
{
    public override void Up()
    {
        if (!Schema.Table("FriendRequests").Exists())
        {
            Create.Table("FriendRequests")
                .WithColumn("Id").AsInt64().Identity().PrimaryKey()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("SenderId").AsInt64().NotNullable()
                .WithColumn("RecipientId").AsInt64().NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable()
                .WithColumn("Status").AsByte().NotNullable();
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_FriendRequests_SenderId")
                .FromTable("FriendRequests").ForeignColumn("SenderId")
                .ToTable("Users").PrimaryColumn("Id");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_FriendRequests_RecipientId")
                .FromTable("FriendRequests").ForeignColumn("RecipientId")
                .ToTable("Users").PrimaryColumn("Id");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_FriendRequests_SenderId")
                .OnTable("FriendRequests")
                .OnColumn("SenderId");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_FriendRequests_UniquePending")
                .OnTable("FriendRequests")
                .OnColumn("SenderId").Ascending()
                .OnColumn("RecipientId").Ascending()
                .WithOptions().Unique();
        }

        if (!Schema.Table("Friendships").Exists())
        {
            Create.Table("Friendships")
                .WithColumn("Id").AsInt64().Identity().PrimaryKey()
                .WithColumn("PublicId").AsGuid().WithDefault(SystemMethods.NewGuid).NotNullable()
                .WithColumn("User1Id").AsInt64().NotNullable()
                .WithColumn("User2Id").AsInt64().NotNullable()
                .WithColumn("CreatedAtUtc").AsDateTime2().NotNullable();
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Friendships_User1Id")
                .FromTable("Friendships").ForeignColumn("User1Id")
                .ToTable("Users").PrimaryColumn("Id");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.ForeignKey("FK_Friendships_User2Id")
                .FromTable("Friendships").ForeignColumn("User2Id")
                .ToTable("Users").PrimaryColumn("Id");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_Friendships_User1Id")
                .OnTable("Friendships")
                .OnColumn("User1Id");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_Friendships_User2Id")
                .OnTable("Friendships")
                .OnColumn("User2Id");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Create.Index("IX_Friendships_UniquePair")
                .OnTable("Friendships")
                .OnColumn("User1Id").Ascending()
                .OnColumn("User2Id").Ascending()
                .WithOptions().Unique();
                
        }
    }

    public override void Down()
    {
        if (Schema.Table("FriendRequests").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_FriendRequests_UniquePending");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_FriendRequests_SenderId");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_FriendRequests_RecipientId");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_FriendRequests_RecipientId");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_FriendRequests_SenderId");
            
            Delete.Table("FriendRequests");
        }

        if (Schema.Table("Friendships").Exists())
        {
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_Friendships_UniquePair");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_Friendships_User2Id");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.Index("IX_Friendships_User1Id");

            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Friendships_User2Id");
            
            IfDatabase("sqlserver", "postgresql", "mysql", "oracle")
                .Delete.ForeignKey("FK_Friendships_User1Id");
            
            Delete.Table("Friendships");
        }
    }
}