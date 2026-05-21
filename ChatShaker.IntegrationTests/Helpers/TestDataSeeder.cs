using System.Data;
using System.Security.Cryptography;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Enums;
using ChatShaker.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.IntegrationTests.Helpers;

public static class TestDataSeeder
{
    private static string[] _roleNames = { "User", "Admin" };

    private static List<User> GetUsers() => new()
    {
        new User
        {
            PublicNick = "dsfdfgdfg",
            FirstName = "dgfdgfdg",
            LastName = "Ddfgdssfsdfoe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Email = "test@test.com",
            IsEmailConfirmed = true,
            LastActivityDateTime = new DateTime(2025, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            AccountInfo = "test",
            CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedAtUtc = null
        },
        new User
        {
            PublicNick = "dsfdfdsfgdfg",
            FirstName = "dgfdgsdfsdffdg",
            LastName = "Ddfgdsdfsdfssfsdfoe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Email = "test2@test.com",
            IsEmailConfirmed = true,
            LastActivityDateTime = new DateTime(2025, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            AccountInfo = "test2",
            CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedAtUtc = null
        }
    };

    public static async Task SeedUser(AppDbContext context)
    {
        var isExists = await context.Users.AnyAsync(x => x.Email == "test@test.com");
        if (isExists)
            return;

        var existingRoles = await context.Roles.ToListAsync();
        var rolesToAdd = new List<Role>();
        foreach (var roleName in _roleNames)
        {
            if (!existingRoles.Any(r => r.RoleName == roleName))
            {
                rolesToAdd.Add(new Role { RoleName = roleName });
            }
        }

        if (rolesToAdd.Any())
        {
            await context.Roles.AddRangeAsync(rolesToAdd);
            await context.SaveChangesAsync();
        }

        await context.SaveChangesAsync();
        var savedRoles = await context.Roles.ToListAsync();

        var users = GetUsers();
        var passwordHasher = new PasswordHasher<User>();

        foreach (var user in users)
        {
            if(user.Email == "test@test.com")
                user.RoleId = savedRoles.FirstOrDefault(x => x.RoleName == "Admin").Id;
            else
                user.RoleId = savedRoles.FirstOrDefault(x => x.RoleName == "User").Id;

            user.PasswordHash = passwordHasher.HashPassword(user, "testPassword-1");

            await context.Users.AddAsync(user);
        }
        await context.SaveChangesAsync();

        foreach(var user in users)
        {
            var verificationToken = new Token
            {
                UserId = user.Id,
                TokenData = Convert.ToHexString(RandomNumberGenerator.GetBytes(64)),
                ExpireDateTime = DateTime.UtcNow.AddHours(3),
                CreatedDateUtc =  DateTime.UtcNow,
                TokenType = TokenType.ActivationToken
            };

            await context.Tokens.AddAsync(verificationToken);
        }

        await context.SaveChangesAsync();
    }
}
