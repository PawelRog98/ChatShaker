using ChatShaker.Domain.Repositories;
using ChatShaker.Infrastructure.Authentication;
using ChatShaker.Infrastructure.Configuration;
using Moq;

namespace ChatShaker.UnitTests.Application.Fixtures;

public class AuthFixture
{
    public JwtSettings jwtSettings { get; }
    public Mock<ITokenRepository> TokenRepository { get; }


    public AuthFixture()
    {
        jwtSettings = new JwtSettings
        {
            JwtKey = "D3FAF8129F3A4A3C8A7D4E914F69B7C1",
            JwtExpireMinutes = 60,
            JwtIssuer = "test@test.com"
        };

        TokenRepository = new Mock<ITokenRepository>();
    }

    public AuthService CreateService() 
        => new AuthService(jwtSettings, TokenRepository.Object);
}
