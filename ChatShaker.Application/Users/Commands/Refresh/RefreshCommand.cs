using AutoMapper;
using ChatShaker.Application.Users.Commands.Shared;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using ChatShaker.Domain.Serivces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatShaker.Application.Users.Commands.Refresh
{
    public class RefreshCommand : IRequest<AuthTokenDto>
    {
        public RefreshCommand(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
        public string RefreshToken { get; set; }
    }

    public class RefreshCommandHandler : IRequestHandler<RefreshCommand, AuthTokenDto>
    {
        private readonly IAuthService _authService;
        private readonly ITokenRepository _tokenRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshCommandHandler(IAuthService authService, IMapper mapper, ITokenRepository tokenRepository, IUnitOfWork unitOfWork)
        {
            _authService = authService;
            _mapper = mapper;
            _tokenRepository = tokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthTokenDto> Handle(RefreshCommand refreshCommand, CancellationToken cancellationToken)
        {
            try
            {
                await _unitOfWork.BeginTransaction(cancellationToken);
                var token = await _tokenRepository.GetTokenDataWithUser(refreshCommand.RefreshToken, cancellationToken);
                if (token == null)
                    throw new BadAuthenticationException("Token is expired");

                var user = token.User;

                await _tokenRepository.DeleteToken(token, cancellationToken);

                var accessTokenResult = await _authService.GenerateJwtToken(user, cancellationToken);

                await _unitOfWork.Commit(cancellationToken);
                var tokenResult = _mapper.Map<AuthTokenDto>(accessTokenResult);
                return tokenResult;
            }
            catch (Exception)
            {
                await _unitOfWork.Rollback(cancellationToken);
                throw;
            }
        }
    }
}
