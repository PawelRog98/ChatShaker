using AutoMapper;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Keys.Commands.SaveNewRotation;

public class SaveNewRotationCommand : IRequest<Unit>
{
    public SaveNewRotationCommand(Guid roomPublicId, List<RotationDto> rotations, long userId)
    {
        RoomPublicId = roomPublicId;
        Rotations = rotations;
        UserId = userId;
    }
    
    public Guid RoomPublicId { get; }
    public List<RotationDto> Rotations { get;}
    public long UserId { get; }
}

public class SaveNewRotationCommandHandler : IRequestHandler<SaveNewRotationCommand, Unit>
{
    private readonly IChatRoomKeyBlobRepository _chatRoomKeyBlobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;
    private readonly IUserPublicKeyRepository _userPublicKeyRepository;
    private readonly IUnitOfWork _unitOfWork;

    private readonly IMapper _mapper;

    public SaveNewRotationCommandHandler(IChatRoomKeyBlobRepository chatRoomKeyBlobRepository,
        IUserRepository userRepository, 
        IChatRoomRepository chatRoomRepository,
        IChatRoomMembershipRepository chatRoomMembershipRepository,
        IUserPublicKeyRepository userPublicKeyRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {  
        _chatRoomKeyBlobRepository = chatRoomKeyBlobRepository;
        _userRepository = userRepository;
        _chatRoomRepository = chatRoomRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
        _userPublicKeyRepository = userPublicKeyRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(SaveNewRotationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);

            var room = await _chatRoomRepository.GetByPublicId(request.RoomPublicId, cancellationToken);
            if (room == null)
                throw new BadRequestException("Room not found");

            var isMember = await _chatRoomMembershipRepository.Exists(room.Id, request.UserId, cancellationToken);
            if (!isMember)
                throw new ForbiddenException("You are not a member of this room");

            var currentMembers = await _chatRoomMembershipRepository.GetByRoomId(room.Id, cancellationToken);
            var memberIds = currentMembers.Select(m => m.UserId).ToList();

            var currentVersion = await _chatRoomRepository.GetNewestRoomVersion(request.RoomPublicId, cancellationToken);
            var newVersion = request.Rotations.First().Version;

            if (newVersion != currentVersion + 1)
            {
                throw new BadRequestException("Key rotation conflict: A newer key version already exists.");
            }

            var users = await _userRepository.GetUsersByPublicId(request.Rotations.Select(x => x.UserId).ToList(),
                cancellationToken);

            if (users.Count() != memberIds.Count || users.Any(u => !memberIds.Contains(u.Id)))
            {
                throw new BadRequestException("Key rotation must include all current room members.");
            }

            var newKeys = new List<ChatRoomKeyBlob>();

            foreach (var newKey in request.Rotations)
            {
                var currentUser = users.FirstOrDefault(x => x.PublicId == newKey.UserId);

                // Verify DeviceId exists for user
                var userIdentities = await _userPublicKeyRepository.GetUserIdentitiesByUserId(currentUser.Id, cancellationToken);
                if (!userIdentities.Any(i => i.DeviceId == newKey.DeviceId))
                {
                    throw new BadRequestException($"Invalid DeviceId for user {newKey.UserId}.");
                }

                newKeys.Add(new ChatRoomKeyBlob
                {
                    UserId = currentUser.Id,
                    ChatRoomId = room.Id, // Set ChatRoomId correctly!
                    EncryptedRoomKey = newKey.EncryptedRoomKey,
                    Version = newKey.Version,
                    DeviceId = newKey.DeviceId,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            await _chatRoomKeyBlobRepository.AddRange(newKeys, cancellationToken);
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