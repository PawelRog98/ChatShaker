using ChatShaker.Application.Common.Behaviors;
using ChatShaker.Application.Users.Commands.Login;
using ChatShaker.Application.Users.Commands.Shared;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Moq;
using System.Threading.Tasks;

namespace ChatShaker.UnitTests.Application.Behaviors;

public class ValidationBehaviorsTests
{
    private readonly List<IValidator<LoginCommand>> _validators;

    public ValidationBehaviorsTests()
    {
        _validators = new List<IValidator<LoginCommand>>();
    }
    [Fact]
    public async Task Handle_CallNext_WhenIsValid()
    {
        var login = new LoginDto
        {
            Email = "user@example.com",
            Password = "Password-1"
        };
        var request = new LoginCommand(login);
        var fakeResponse = new AuthTokenDto
        {
            AccessToken = "Test",
            RefreshToken = "TestRefresh",
            UserNick = "Test"
        };

        var validator = new LoginCommandValidator();
        _validators.Add(validator);

        var behavior = new ValidationBehavior<LoginCommand, AuthTokenDto>(_validators);

        bool wasCalled = false;
        RequestHandlerDelegate<AuthTokenDto> next = (ct) =>
            {
                wasCalled = true;
                return Task.FromResult(fakeResponse);
            };

        var act = await behavior.Handle(request, next, CancellationToken.None);

        act.Should().BeEquivalentTo(fakeResponse);
        wasCalled.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ThrowValidationException_WhenEmailInvalid()
    {
        var login = new LoginDto
        {
            Email = "",
            Password = "Password-1"
        };
        var request = new LoginCommand(login);

        var validator = new LoginCommandValidator();
        _validators.Add(validator);

        var behavior = new ValidationBehavior<LoginCommand, AuthTokenDto>(_validators);

        bool wasCalled = false;
        RequestHandlerDelegate<AuthTokenDto> next = (ct) =>
        {
            wasCalled = true;
            return Task.FromResult(new AuthTokenDto());
        };

        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        var errors = exception.Which.Errors;

        errors.Should().HaveCount(2);
        errors.Should().Contain(p => p.PropertyName == "Login.Email");
        wasCalled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ThrowValidationException_WhenAllInvalid()
    {
        var login = new LoginDto
        {
            Email = "",
            Password = ""
        };
        var request = new LoginCommand(login);

        var validator = new LoginCommandValidator();
        _validators.Add(validator);

        var behavior = new ValidationBehavior<LoginCommand, AuthTokenDto>(_validators);

        bool wasCalled = false;
        RequestHandlerDelegate<AuthTokenDto> next = (ct) =>
        {
            wasCalled = true;
            return Task.FromResult(new AuthTokenDto());
        };

        var act = async () => await behavior.Handle(request, next, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<ValidationException>();
        var errors = exception.Which.Errors;

        errors.Should().HaveCount(4);
        errors.Should().Contain(p => p.PropertyName == "Login.Email");
        errors.Should().Contain(p => p.PropertyName == "Login.Password");
        wasCalled.Should().BeFalse();
    }
}
