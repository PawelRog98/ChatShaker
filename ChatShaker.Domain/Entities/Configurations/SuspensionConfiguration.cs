using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Domain.Entities.Configurations
{
    public class SuspensionConfiguration : IEntityTypeConfiguration<Suspension>
    {
        public void Configure(EntityTypeBuilder<Suspension> builder)
        {
            builder.HasOne(u => u.User)
                .WithMany(t => t.Suspensions)
                .HasForeignKey(u => u.UserId);
        }
    }
}
