using FluentValidation;

namespace ChatShaker.Application.Users.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x=>x.Register).SetValidator(new RegisterDtoValidator());
    }
}
