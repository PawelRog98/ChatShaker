using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Enums;
using ChatShaker.Infrastructure.Repositories;
using ChatShaker.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ChatShaker.IntegrationTests.Infrastructure;

[Collection("IntegrationTests")]
public class TokenRepositoryTest : IntegrationTestBase
{
    public TokenRepositoryTest(IntegrationTestsWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task GetActualActivationTokenForUser_ShouldNotThrowTranslationException()
    {
        var repository = new TokenRepository(Context);
        var user = new User
        {
            PublicNick = "testuser",
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            PasswordHash = "hash",
            IsEmailConfirmed = false,
            CreatedAtUtc = DateTime.UtcNow,
            DateOfBirth = new DateTime(1990, 1, 1),
            LastActivityDateTime = DateTime.UtcNow
        };
        await Context.Users.AddAsync(user);
        
        var token = new Token
        {
            TokenData = "activation-token",
            ExpireDateTime = DateTime.UtcNow.AddHours(1),
            TokenType = TokenType.ActivationToken,
            User = user,
            CreatedDateUtc = DateTime.UtcNow
        };
        await Context.Tokens.AddAsync(token);
        await Context.SaveChangesAsync();

        Func<Task> act = async () => await repository.GetActualActivationTokenForUser("activation-token", "test@example.com", CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
