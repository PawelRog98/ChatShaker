using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatShaker.Domain.Entities.Configurations;

public class ChatRoomKeyBlobConfiguration : IEntityTypeConfiguration<ChatRoomKeyBlob>
{
    public void Configure(EntityTypeBuilder<ChatRoomKeyBlob> builder)
    {
        builder.HasKey(x=> new {x.ChatRoomId, x.UserId});
    }
    
}
