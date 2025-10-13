using ChatShaker.Core.Models.Authentication;
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
        var userModel = new UserModel
        {
            Id = 123,
            FirstName = "dgfdgfdg",
            LastName = "Ddfgdssfsdfoe",
            PublicNick = "dsfdfgdfg",
            RoleName = "User"
        };
        

        _authFixture.TokenRepository
            .Setup(t => t.CreateToken(It.IsAny<Token>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var act = await _authService.GenerateJwtToken(userModel, CancellationToken.None);

        act.Should().NotBeNull();

        _authFixture.TokenRepository
            .Verify(t => t.CreateToken(It.IsAny<Token>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
