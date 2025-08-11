using ChatShaker.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Infrastructure.Data
{
    public class EntitiesBuilderConfiguration
    {
        public void Configure(ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var clrType = entityType.ClrType;

                if (typeof(ICommonData).IsAssignableFrom(clrType))
                {
                    modelBuilder.Entity(clrType)
                        .Property("PublicId")
                        .HasDefaultValueSql("NEWID()");
                }
            }
        }
    }
}
