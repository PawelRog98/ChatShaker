using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Tokens.Commands.ActivateAccount;

public class ConfirmAccountCommand : IRequest<Unit>
{
    public ConfirmAccountCommand(ConfirmAccountDto confirmAccountDto)
    {
        ConfirmAccountDto = confirmAccountDto;
    }
    
    public ConfirmAccountDto ConfirmAccountDto { get; set; }
}

public class ConfirmAccountCommandHandler : IRequestHandler<ConfirmAccountCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;

    public ConfirmAccountCommandHandler(IUnitOfWork unitOfWork, IUserRepository userRepository,  ITokenRepository tokenRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
    }

    public async Task<Unit> Handle(ConfirmAccountCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);
            var token = await _tokenRepository.GetActualActivationTokenForUser(request.ConfirmAccountDto.Token, request.ConfirmAccountDto.Email, cancellationToken);

            if (token == null)
                throw new BadAuthenticationException("Token is wrong");
            
            var user = await _userRepository.GetUserByEmail(request.ConfirmAccountDto.Email, cancellationToken);
            
            user.IsEmailConfirmed = true;
            
            await _unitOfWork.Commit(cancellationToken);
            
            return  Unit.Value;

        }
        catch (Exception ex)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}