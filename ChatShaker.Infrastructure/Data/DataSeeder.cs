using ChatShaker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Infrastructure.Data
{
    public class DataSeeder
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public DataSeeder(AppDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task Seed()
        {
            await DbHealtCheck();

            if (!await _context.Roles.AnyAsync())
            {
                await _context.Roles.AddRangeAsync(GetRoles());
                await _context.SaveChangesAsync();
            }

            if (!await _context.Users.AnyAsync())
            {
                var adminRole = await _context.Roles
                    .FirstOrDefaultAsync(x => x.RoleName == "Administrator");

                if (adminRole == null)
                    throw new Exception("Role Admin not found in db");

                await _context.Users.AddRangeAsync(GetUsers(adminRole.Id));
                await _context.SaveChangesAsync();
            }
        }

        private IEnumerable<Role> GetRoles()
        {
            return new List<Role>()
            {
                new Role
                {
                    RoleName = "User"
                },
                new Role
                {
                    RoleName = "Administrator"
                }
            };
        }

        private IEnumerable<User> GetUsers(long adminRoleId)
        {
            var users = new List<User>()
            {
                new User
                {
                    Email = "admin1@admin.com",
                    PublicNick = "Admin",
                    FirstName = "Name",
                    LastName = "LastName",
                    PasswordHash = "Password-1",
                    DateOfBirth = DateTime.UtcNow.AddYears(-20),
                    IsEmailConfirmed = true,
                    AccountInfo = "Admin",
                    RoleId = adminRoleId
                }
            };
            foreach (var user in users)
            {
                var password = _passwordHasher.HashPassword(user, user.PasswordHash);
                user.PasswordHash = password;
            }

            return users;
        }

        private async Task DbHealtCheck()
        {
            int maxRetries = 12;
            int delaySeconds = 15;
            bool isConnected = false;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await _context.Database.OpenConnectionAsync();
                    await _context.Database.CloseConnectionAsync();

                    isConnected = true;
                    Console.WriteLine($"Connected to DB in: {stopwatch.Elapsed}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SQL Failed in {i + 1} attempt");
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }


            }
            stopwatch.Stop();

            if (!isConnected)
            {
                throw new Exception("Database is not available");
            }
        }
    }
}
