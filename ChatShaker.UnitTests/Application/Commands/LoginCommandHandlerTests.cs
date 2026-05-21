using AutoMapper;
using ChatShaker.Application.Users.Commands.Login;
using ChatShaker.Application.Users.Commands.Shared;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Models.Authentication;
using ChatShaker.Domain.Repositories;
using ChatShaker.Domain.Serivces;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace ChatShaker.UnitTests.Application.Commands;

public class LoginCommandHandlerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher<User>> _passwordHasherMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _mapperMock = new Mock<IMapper>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher<User>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _handler = new LoginCommandHandler(
            _authServiceMock.Object,
            _mapperMock.Object,
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _unitOfWorkMock.Object
            );
    }

    [Fact]
    public async Task Handle_ReturnCorrectResult_WhenIsValid()
    {
        #region DataInitialize
        var loginDto = new LoginDto
        {
            Email = "test@test.com",
            Password = "dasdsdasdasd"
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
            ModifiedAtUtc = null
        };
        var userModel = new UserModel
        {
            Id = 1,
            FirstName = "dgfdgfdg",
            LastName = "Ddfgdssfsdfoe",
            PublicNick = "dsfdfgdfg"
        };

        var token = new AuthTokenModel
        {
            AccessToken = "sgdgsdfdgdfgdfg",
            RefreshToken = "fsdfdsfsf",
            UserNick = "dsfdfgdfg"
        };
        var tokenDto = new AuthTokenDto
        {
            AccessToken = "sgdgsdfdgdfgdfg",
            RefreshToken = "fsdfdsfsf",
            UserNick = "dsfdfgdfg"
        };

        var loginCommand = new LoginCommand(loginDto);
        #endregion

        _userRepositoryMock
            .Setup(r => r.GetUserByEmail(loginCommand.Login.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(m => m.Map<UserModel>(user))
            .Returns(userModel);

        _passwordHasherMock
            .Setup(p => p.VerifyHashedPassword(user, user.PasswordHash, loginCommand.Login.Password))
            .Returns(PasswordVerificationResult.Success);

        _authServiceMock
            .Setup(a => a.GenerateJwtToken(user, It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);

        _mapperMock
            .Setup(m => m.Map<AuthTokenDto>(token))
            .Returns(tokenDto);

        var act = await _handler.Handle(loginCommand, CancellationToken.None);

        act.Should().NotBeNull();
        _userRepositoryMock.Verify(r => r.GetUserByEmail(loginCommand.Login.Email, It.IsAny<CancellationToken>()), Times.Once());
        _passwordHasherMock.Verify(p => p.VerifyHashedPassword(user, user.PasswordHash, loginCommand.Login.Password), Times.Once());
        _authServiceMock.Verify(a => a.GenerateJwtToken(user, It.IsAny<CancellationToken>()), Times.Once());
        _unitOfWorkMock.Verify(u => u.BeginTransaction(It.IsAny<CancellationToken>()), Times.Once());
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Handle_UserNotFound_ThrowsBadAuthenticationException()
    {
        #region DataInitialize
        var loginDto = new LoginDto
        {
            Email = "test@test.com",
            Password = "dasdsdasdasd"
        };

        var loginCommand = new LoginCommand(loginDto);
        #endregion

        _userRepositoryMock
            .Setup(r => r.GetUserByEmail(loginCommand.Login.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = async () => await _handler.Handle(loginCommand, CancellationToken.None);

        await act.Should().ThrowAsync<BadAuthenticationException>()
            .WithMessage("Invalid user data.");

        _userRepositoryMock.Verify(r => r.GetUserByEmail(loginCommand.Login.Email, It.IsAny<CancellationToken>()), Times.Once());
        _unitOfWorkMock.Verify(u => u.BeginTransaction(It.IsAny<CancellationToken>()), Times.Once());
        _unitOfWorkMock.Verify(u => u.Rollback(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Handle_PasswordIncorrect_ThrowsBadAuthenticationException()
    {
        #region DataInitialize
        var loginDto = new LoginDto
        {
            Email = "test@test.com",
            Password = "dasdsdasdasd"
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
            PasswordHash = "Hashed1",
            IsEmailConfirmed = true,
            LastActivityDateTime = new DateTime(2025, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            AccountInfo = "test",
            CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedAtUtc = null
        };

        var loginCommand = new LoginCommand(loginDto);
        #endregion

        _userRepositoryMock
            .Setup(r => r.GetUserByEmail(loginCommand.Login.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(p => p.VerifyHashedPassword(user, user.PasswordHash, loginCommand.Login.Password))
            .Returns(PasswordVerificationResult.Failed);

        var act = async () => await _handler.Handle(loginCommand, CancellationToken.None);

        await act.Should().ThrowAsync<BadAuthenticationException>()
            .WithMessage("Invalid user data.");

        _userRepositoryMock.Verify(r => r.GetUserByEmail(loginCommand.Login.Email, It.IsAny<CancellationToken>()), Times.Once());
        _passwordHasherMock.Verify(p => p.VerifyHashedPassword(user, user.PasswordHash, loginCommand.Login.Password), Times.Once());
        _unitOfWorkMock.Verify(u => u.BeginTransaction(It.IsAny<CancellationToken>()), Times.Once());
        _unitOfWorkMock.Verify(u => u.Rollback(It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Handle_UnconfirmedEmail_ThrowsNotActiveUserException()
    {
        #region DataInitialize
        var loginDto = new LoginDto
        {
            Email = "test@test.com",
            Password = "dasdsdasdasd"
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
            PasswordHash = "Hashed1",
            IsEmailConfirmed = false,
            LastActivityDateTime = new DateTime(2025, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            AccountInfo = "test",
            CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedAtUtc = null
        };

        var loginCommand = new LoginCommand(loginDto);
        #endregion

        _userRepositoryMock
            .Setup(r => r.GetUserByEmail(loginCommand.Login.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var act = async () => await _handler.Handle(loginCommand, CancellationToken.None);

        await act.Should().ThrowAsync<NotActiveUserException>();

        _userRepositoryMock.Verify(r => r.GetUserByEmail(loginCommand.Login.Email, It.IsAny<CancellationToken>()), Times.Once());
        _unitOfWorkMock.Verify(u => u.BeginTransaction(It.IsAny<CancellationToken>()), Times.Once());
        _unitOfWorkMock.Verify(u => u.Rollback(It.IsAny<CancellationToken>()), Times.Once());
    }

}
