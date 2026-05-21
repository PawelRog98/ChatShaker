using AutoMapper;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Enums;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ChatShaker.Application.Events;
using ChatShaker.Application.Interfaces;

namespace ChatShaker.Application.Users.Commands.Register
{
    public class RegisterCommand : IRequest<Unit>
    {
        public RegisterCommand(RegisterDto register)
        {
            Register = register;
        }
        public RegisterDto Register { get; set; }
    }

    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Unit>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ITokenRepository _tokenRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly ICodeGenerationService _codeGeneration;
        private readonly IRoleRepository _roleRepository;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        public RegisterCommandHandler(IUserRepository userRepository 
            ,IMapper mapper
            ,ITokenRepository tokenRepository
            ,IPasswordHasher<User> passwordHasher
            ,ICodeGenerationService codeGeneration
            ,IRoleRepository roleRepository
            ,IMediator mediator
            ,IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _tokenRepository = tokenRepository;
            _passwordHasher = passwordHasher;
            _codeGeneration = codeGeneration;
            _roleRepository = roleRepository;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(RegisterCommand command, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransaction(cancellationToken);
                var existedUser = await _userRepository.GetUserByEmail(command.Register.Email, cancellationToken);

                if (existedUser != null)
                    throw new BadAuthenticationException("A user with such an email already exists.");

                var defaultRole = await _roleRepository.GetIdByName("User", cancellationToken);

                if (!command.Register.DateOfBirth.HasValue)
                    throw new BadRequestException("Date of birth is required.");

                var user = new User
                {
                    Email = command.Register.Email,
                    PublicNick = command.Register.PublicNick,
                    FirstName = command.Register.FirstName,
                    LastName = command.Register.LastName,
                    DateOfBirth = command.Register.DateOfBirth.Value,
                    UserInvitationCode = _codeGeneration.GenerateCode(),
                    RoleId = defaultRole.Id
                };

                var password = _passwordHasher.HashPassword(user, command.Register.Password);
                var verificaionToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(10));

                user.PasswordHash = password;

                await _userRepository.Add(user, cancellationToken);
                await _unitOfWork.SaveChanges(cancellationToken);

                var verificationToken = new Token
                {
                    UserId = user.Id,
                    TokenData = verificaionToken,
                    ExpireDateTime = DateTime.UtcNow.AddHours(3),
                    CreatedDateUtc =  DateTime.UtcNow,
                    TokenType = TokenType.ActivationToken
                };

                await _tokenRepository.Add(verificationToken, cancellationToken);

                await _unitOfWork.Commit(cancellationToken);

                await _mediator.Publish(new UserRegisteredEvent(user.Email, verificationToken.TokenData), cancellationToken);

                return Unit.Value;
            }
            catch (Exception)
            {
                await _unitOfWork.Rollback(cancellationToken);
                throw;
            }
        }
    }
}
