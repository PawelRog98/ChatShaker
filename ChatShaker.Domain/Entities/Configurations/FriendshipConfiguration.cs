using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatShaker.Domain.Entities.Configurations;

public class FriendshipConfiguration : IEntityTypeConfiguration<Friendship>
{
    public void Configure(EntityTypeBuilder<Friendship> builder)
    {
        builder.HasOne(x=>x.User1)
            .WithMany()
            .HasForeignKey(x => x.User1Id);
        
        builder.HasOne(x=>x.User2)
            .WithMany()
            .HasForeignKey(x => x.User2Id);
    }
}