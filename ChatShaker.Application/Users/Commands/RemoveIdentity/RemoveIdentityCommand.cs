using ChatShaker.Application.Interfaces;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Users.Commands.RemoveIdentity;

public class RemoveIdentityCommand : IRequest<Unit>
{
    public RemoveIdentityCommand(string deviceId, long userId)
    {
        DeviceId = deviceId;
        UserId = userId;
    }

    public string DeviceId { get; }
    public long UserId { get; }
}

public class RemoveIdentityCommandHandler : IRequestHandler<RemoveIdentityCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserPublicKeyRepository _userPublicKeyRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatNotifier _chatNotifier;

    public RemoveIdentityCommandHandler(IUnitOfWork unitOfWork, 
        IUserPublicKeyRepository userPublicKeyRepository,
        IUserRepository userRepository,
        IChatNotifier chatNotifier)
    {
        _unitOfWork = unitOfWork;
        _userPublicKeyRepository = userPublicKeyRepository;
        _userRepository = userRepository;
        _chatNotifier = chatNotifier;
    }

    public async Task<Unit> Handle(RemoveIdentityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);

            var existingKeys = await _userPublicKeyRepository.GetUserIdentitiesByUserId(request.UserId, cancellationToken);
            var deviceToRemove = existingKeys.FirstOrDefault(k => k.DeviceId == request.DeviceId);

            if (deviceToRemove == null)
            {
                throw new BadRequestException("Device identity not found.");
            }

            await _userPublicKeyRepository.Remove(deviceToRemove, cancellationToken);
            await _unitOfWork.Commit(cancellationToken);

            var user = await _userRepository.GetUserById(request.UserId, cancellationToken);
            if (user != null)
            {
                await _chatNotifier.UserIdentityChanged(user.PublicId, cancellationToken);
            }

            return Unit.Value;
        }
        catch (Exception)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}
