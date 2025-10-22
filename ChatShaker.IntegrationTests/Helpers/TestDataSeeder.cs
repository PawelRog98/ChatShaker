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
    public static async Task SeedUser(AppDbContext context)
    {
        var isExists = await context.Users.AnyAsync(x=>x.Email == "test@test.com");
        Console.WriteLine($"Users: {isExists}");
        if (isExists)
            return;

        var role = new Role
        {
            RoleName = "User",

        };

        await context.Roles.AddAsync(role);
        await context.SaveChangesAsync();

        var passwordHasher = new PasswordHasher<User>();

        var user = new User
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
            ModifiedAtUtc = null,
            RoleId = role.Id
        };

        user.PasswordHash = passwordHasher.HashPassword(user, "testPassword-1");
        var verificaionToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();

        var verificationToken = new Token
        {
            UserId = user.Id,
            TokenData = verificaionToken,
            ExpireDateTime = DateTime.UtcNow.AddHours(3),
            TokenType = TokenType.ActivationToken
        };

        await context.Tokens.AddAsync(verificationToken);
        await context.SaveChangesAsync();
    }
}
