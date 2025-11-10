using AutoMapper;
using ChatShaker.Application.Users.Commands.Shared;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using ChatShaker.Domain.Serivces;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Application.Users.Commands.Login
{
    public class LoginCommand : IRequest<AuthTokenDto>
    {
        public LoginCommand(LoginDto login) 
        {
            Login = login;
        }
        public LoginDto Login { get; set; }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthTokenDto>
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;

        public LoginCommandHandler(IAuthService authService, IMapper mapper, IUserRepository userRepository, IPasswordHasher<User> passwordHasher)
        {
            _authService = authService;
            _mapper = mapper;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
        }

        public async Task<AuthTokenDto> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetUserByEmail(command.Login.Email, cancellationToken);

            if (user == null)
            {
                throw new BadAuthenticationException("Invalid user data.");
            }

            if (user.IsEmailConfirmed == false)
            {
                throw new NotActiveUserException();
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, command.Login.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new BadAuthenticationException("Invalid user data.");
            }
            var token = await _authService.GenerateJwtToken(user, cancellationToken);

            var resultToken = _mapper.Map<AuthTokenDto>(token);

            return resultToken;
        }
    }
}
