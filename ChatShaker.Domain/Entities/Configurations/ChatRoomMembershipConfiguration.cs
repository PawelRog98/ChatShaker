using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatShaker.Domain.Entities.Configurations;

public class ChatRoomMembershipConfiguration : IEntityTypeConfiguration<ChatRoomMembership>
{
    public void Configure(EntityTypeBuilder<ChatRoomMembership> builder)
    {
        builder.HasKey(x => new {x.UserId, x.ChatRoomId});
    }
}
