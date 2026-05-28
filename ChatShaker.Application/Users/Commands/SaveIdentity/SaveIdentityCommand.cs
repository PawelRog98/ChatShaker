using ChatShaker.Application.Interfaces;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
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
    private readonly IUserRepository _userRepository;
    private readonly IChatNotifier _chatNotifier;

    public SaveIdentityCommandHandler(IUnitOfWork unitOfWork, 
        IUserPublicKeyRepository userPublicKeyRepository,
        IUserRepository userRepository,
        IChatNotifier chatNotifier)
    {
        _unitOfWork = unitOfWork;
        _userPublicKeyRepository = userPublicKeyRepository;
        _userRepository = userRepository;
        _chatNotifier = chatNotifier;
    }

    public async Task<Unit> Handle(SaveIdentityCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);

            var existingKeys = await _userPublicKeyRepository.GetUserIdentitiesByUserId(request.UserId, cancellationToken);

            if (request.UserKeyData.ClearOtherDevices)
            {
                var otherKeys = existingKeys.Where(k => k.DeviceId != request.UserKeyData.DeviceId).ToList();
                foreach (var key in otherKeys)
                {
                    await _userPublicKeyRepository.Remove(key, cancellationToken);
                }
                existingKeys = existingKeys.Where(k => k.DeviceId == request.UserKeyData.DeviceId).ToList();
            }

            var existingDeviceKey = existingKeys.FirstOrDefault(k => k.DeviceId == request.UserKeyData.DeviceId);

            if (existingDeviceKey != null)
            {
                if (existingDeviceKey.PublicKey != request.UserKeyData.PublicKey)
                {
                    throw new BadRequestException("A different public key is already registered for this device. If you have reset your device, please use a new device identifier.");
                }

                existingDeviceKey.CreatedAtUtc = DateTime.UtcNow;
                await _userPublicKeyRepository.Update(existingDeviceKey, cancellationToken);
            }
            else
            {
                if (existingKeys.Count >= 5)
                {
                    throw new BadRequestException("Maximum number of devices (5) reached. Please remove an old device before adding a new one.");
                }

                var userIdentity = new UserPublicKey
                {
                    UserId = request.UserId,
                    PublicKey = request.UserKeyData.PublicKey,
                    DeviceId = request.UserKeyData.DeviceId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _userPublicKeyRepository.Add(userIdentity, cancellationToken);
            }

            await _unitOfWork.Commit(cancellationToken);

            var user = await _userRepository.GetUserById(request.UserId, cancellationToken);
            if (user != null)
            {
                await _chatNotifier.UserIdentityChanged(user.PublicId, cancellationToken);
            }

            return Unit.Value;
        }
        catch (Exception ex)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}