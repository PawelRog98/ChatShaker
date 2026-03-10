using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Users.Commands.SaveIdentity;

public class SaveIdentityCommand : IRequest<Unit>
{
    public SaveIdentityCommand(UserKeyDataDto userKeyDataDto, long userId)
    {
        UserKeyData =  userKeyDataDto;
        UserId = userId;
    }
    
    public UserKeyDataDto UserKeyData { get; set; }
    public long UserId { get; set; }
}

public class SaveIdentityCommandHandler : IRequestHandler<SaveIdentityCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserPublicKeyRepository _userPublicKeyRepository;
    
    public SaveIdentityCommandHandler(IUnitOfWork unitOfWork,  IUserPublicKeyRepository userPublicKeyRepository)
    {
        _unitOfWork = unitOfWork;
        _userPublicKeyRepository = userPublicKeyRepository;
    }
    
    public async Task<Unit> Handle(SaveIdentityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);

            var userIdentity = new UserPublicKey
            {
                UserId = request.UserId,
                PublicKey = request.UserKeyData.PublicKey,
                DeviceId = request.UserKeyData.DeviceId,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _userPublicKeyRepository.SaveIdentity(userIdentity, cancellationToken);
            
            await _unitOfWork.Commit(cancellationToken);
            
            return Unit.Value;
        }
        catch 
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}