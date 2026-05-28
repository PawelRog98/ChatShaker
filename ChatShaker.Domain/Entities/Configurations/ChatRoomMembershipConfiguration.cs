using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatShaker.Domain.Entities.Configurations;

public class ChatRoomMembershipConfiguration : IEntityTypeConfiguration<ChatRoomMembership>
{
    public void Configure(EntityTypeBuilder<ChatRoomMembership> builder)
    {
        builder.HasKey(x => new {x.ChatRoomId, x.UserId});
        
        builder.HasOne(x => x.ChatRoom)
            .WithMany(x => x.ChatRoomMemberships)
            .HasForeignKey(x => x.ChatRoomId);
        
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
        
        builder.HasOne(x=>x.AddedBy)
            .WithMany()
            .HasForeignKey(x => x.AddedById);
    }
}
