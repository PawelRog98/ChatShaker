using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatShaker.Domain.Entities.Configurations;

public class FriendRequestConfiguration : IEntityTypeConfiguration<FriendRequest>
{
    public void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        builder.HasOne(x => x.Recipient)
            .WithMany(t=>t.ReceivedFriendRequests)
            .HasForeignKey(x => x.RecipientId);
        
        builder.HasOne(x => x.Sender)
            .WithMany(t => t.SentFriendRequests)
            .HasForeignKey(x => x.SenderId);
    }
}