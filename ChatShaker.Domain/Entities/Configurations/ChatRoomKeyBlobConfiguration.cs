using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatShaker.Domain.Entities.Configurations;

public class ChatRoomKeyBlobConfiguration : IEntityTypeConfiguration<ChatRoomKeyBlob>
{
    public void Configure(EntityTypeBuilder<ChatRoomKeyBlob> builder)
    {
        builder.HasKey(x=> new {x.ChatRoomId, x.UserId, x.DeviceId});
        
        builder.HasOne(x => x.ChatRoom)
            .WithMany(x => x.ChatRoomKeyBlobs)
            .HasForeignKey(x => x.ChatRoomId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);
    }
    
}
