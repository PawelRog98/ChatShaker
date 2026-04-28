using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Entities.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions dbContextOptions) : base(dbContextOptions) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<UserPublicKey> UserPublicKeys { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatRoomMembership> ChatRoomMemberships { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ChatRoomKeyBlob> ChatRoomKeyBlobs { get; set; }
        public DbSet<MessageStatus> MessageStatuses { get; set; }
        public DbSet<FriendRequest> FriendRequests { get; set; }
        public DbSet<Friendship> Friendships { get; set; }
        public DbSet<FileResource> FileResources { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var configurator = new EntitiesBuilderConfiguration();
            configurator.Configure(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(TokenConfiguration).Assembly);
        }
    }
}
