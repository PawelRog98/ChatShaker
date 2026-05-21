using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Authentication;
using ChatShaker.UnitTests.Application.Fixtures;
using FluentAssertions;
using Moq;

namespace ChatShaker.UnitTests.Infrastructure.Services;

public class AuthServiceTests : IClassFixture<AuthFixture>
{

    private readonly AuthFixture _authFixture;
    private readonly AuthService _authService;
    public AuthServiceTests(AuthFixture authFixture)
    {
        _authFixture = authFixture;
        _authService = authFixture.CreateService();
    }

    [Fact]
    public async Task GenerateJwtToken_ReturnCorrectValue_WhenCorrect()
    {
        var role = new Role
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            RoleName = "User"
        };
        var user = new User
        {
            Id = 1,
            PublicId = Guid.NewGuid(),
            PublicNick = "dsfdfgdfg",
            FirstName = "dgfdgfdg",
            LastName = "Ddfgdssfsdfoe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Email = "test@test.com",
            PasswordHash = "Hashed",
            IsEmailConfirmed = true,
            LastActivityDateTime = new DateTime(2025, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            AccountInfo = "test",
            CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedAtUtc = null,
            RoleId = 1,
            Role = role
        };


        _authFixture.TokenRepository
            .Setup(t => t.Add(It.IsAny<Token>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var act = await _authService.GenerateJwtToken(user, CancellationToken.None);

        act.Should().NotBeNull();

        _authFixture.TokenRepository
            .Verify(t => t.Add(It.IsAny<Token>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
