using ChatShaker.Domain.Entities;
using ChatShaker.Infrastructure.Repositories;
using ChatShaker.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ChatShaker.IntegrationTests.Infrastructure;

public class UserRepositoryTest : IntegrationTestBase
{
    public UserRepositoryTest(IntegrationTestsWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task CreateUser_SaveUser_IsValid()
    {
        var repository = new UserRepository(Context);
        var user = new User
        {
            PublicNick = "dsfdfgdfg",
            FirstName = "dgfdgfdg",
            LastName = "Ddfgdssfsdfoe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Email = "test2@test.com",
            PasswordHash = "Hashed",
            IsEmailConfirmed = true,
            LastActivityDateTime = new DateTime(2025, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            AccountInfo = "test",
            CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedAtUtc = null
        };

        await repository.SaveNewUser(user, CancellationToken.None);

        var dbuser = await Context.Users.FirstOrDefaultAsync(x => x.Email == "test@test.com");

        dbuser.Should().NotBeNull();
    }

    [Fact]
    public async Task GetUserByEmail_ReturnsUser_WhenExists()
    {
        var repository = new UserRepository(Context);
        var user = new User
        {
            PublicNick = "dsfdfgdfg",
            FirstName = "dgfdgfdg",
            LastName = "Ddfgdssfsdfoe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Email = "testgetuser@test.com",
            PasswordHash = "Hashed",
            IsEmailConfirmed = true,
            LastActivityDateTime = new DateTime(2025, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            AccountInfo = "test",
            CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedAtUtc = null
        };

        await Context.Users.AddAsync(user, CancellationToken.None);
        await Context.SaveChangesAsync();

        var foundUser = await repository.GetUserByEmail("testgetuser@test.com", CancellationToken.None);

        foundUser.Should().NotBeNull();
        foundUser.Email.Should().Be("testgetuser@test.com");
    }

}
