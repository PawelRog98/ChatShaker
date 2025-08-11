using ChatShaker.ChatMauiApp.Models.Dto;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.ChatMauiApp.Models.Validators
{
    public class RegisterDtoValidator : AbstractValidator<RegisterDto>
    {
        public RegisterDtoValidator() 
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();

            RuleFor(x => x.Password).MinimumLength(8);
            RuleFor(x => x.ConfirmPassword).Equal(y => y.Password);
            RuleFor(x=>x.DateOfBirth).NotEmpty();
        }
    }
}
