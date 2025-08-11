using ChatShaker.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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
            var ifRolesExixts = await _context.Roles.AsNoTracking().AnyAsync();
            var ifUsersExists = await _context.Users.AsNoTracking().AnyAsync();

            if (!ifRolesExixts)
            {
                await _context.Roles.AddRangeAsync(GetRoles());
                await _context.SaveChangesAsync();
            }
            if (!ifUsersExists)
            {
                var adminRole = await _context.Roles.FirstOrDefaultAsync(x=>x.RoleName == "Administrator");

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
            var users =  new List<User>()
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
    }
}
