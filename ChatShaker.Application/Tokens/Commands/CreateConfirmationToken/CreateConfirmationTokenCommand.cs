using System.Security.Cryptography;
using ChatShaker.Application.Events;
using ChatShaker.Domain.Enums;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using ChatShaker.Domain.Serivces;
using MediatR;

namespace ChatShaker.Application.Tokens.Commands.CreateConfirmationToken;

public class CreateConfirmationTokenCommand : IRequest<Unit>
{
    public CreateConfirmationTokenCommand(CreateConfirmationTokenDto emailDto)
    {
        EmailDto = emailDto;
    }
    
    public CreateConfirmationTokenDto EmailDto { get; set; }
}

public class CreateConfirmationTokenCommandHandler : IRequestHandler<CreateConfirmationTokenCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;
    private readonly IMediator _mediator;

    public CreateConfirmationTokenCommandHandler(IUnitOfWork unitOfWork, IUserRepository userRepository,  IAuthService authService,  IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _authService = authService;
        _mediator = mediator;
    }

    public async Task<Unit> Handle(CreateConfirmationTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);
            
            var user = await _userRepository.GetUserByEmail(request.EmailDto.Email, cancellationToken);

            if (user == null)
                throw new BadRequestException("User not found");
            
            var verificaionToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(10));
            
            var token = await _authService.CreateToken(user.Id, TokenType.ActivationToken, DateTime.UtcNow.AddHours(3), verificaionToken, cancellationToken);
            
            await _mediator.Publish(new UserRegisteredEvent(user.Email, token.TokenData), cancellationToken);
            
            await _unitOfWork.Commit(cancellationToken);
            
            return Unit.Value;
        }
        catch (Exception ex)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}