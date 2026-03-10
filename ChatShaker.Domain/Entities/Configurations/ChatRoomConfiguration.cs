using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatShaker.Domain.Entities.Configurations;

public class ChatRoomConfiguration : IEntityTypeConfiguration<ChatRoom>
{
    public void Configure(EntityTypeBuilder<ChatRoom> builder)
    {
        builder.HasOne(x=>x.Host)
            .WithMany()
            .HasForeignKey(x=>x.HostId);
    }
}