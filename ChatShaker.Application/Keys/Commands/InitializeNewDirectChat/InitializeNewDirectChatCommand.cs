using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Keys.Commands.InitializeNewDirectChat;

public class InitializeNewDirectChatCommand : IRequest<Unit>
{
    public InitializeNewDirectChatCommand(ChatRoomDto chatRoomDto, long userId)
    {
        ChatRoomDto = chatRoomDto;
        UserId = userId;
    }
    
    public ChatRoomDto ChatRoomDto { get; set; }
    public long UserId { get; set; }
}

public class InitializeNewDirecChatCommandHandler : IRequestHandler<InitializeNewDirectChatCommand, Unit>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserPublicKeyRepository _userPublicKeyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InitializeNewDirecChatCommandHandler(IChatRoomRepository chatRoomRepository,
        IChatRoomMembershipRepository chatRoomMembershipRepository,
        IUserRepository userRepository,
        IUserPublicKeyRepository userPublicKeyRepository,
        IUnitOfWork unitOfWork)
    {
        _chatRoomRepository = chatRoomRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
        _userRepository = userRepository;
        _userPublicKeyRepository = userPublicKeyRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Unit> Handle(InitializeNewDirectChatCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);

            var chatRoom = await _chatRoomRepository.GetByPublicId(request.ChatRoomDto.ChatRoomPublicId, cancellationToken);
            if (chatRoom == null)
                throw new BadRequestException("Room not found");

            if (chatRoom.IsInitialized)
                throw new BadRequestException("Room is already initialized. Use key rotation or add member to manage keys.");

            var isMember = await _chatRoomMembershipRepository.Exists(chatRoom.Id, request.UserId, cancellationToken);
            if (!isMember)
                throw new ForbiddenException("You are not a member of this room");

            var usersIds = request.ChatRoomDto.ChatRoomKeyBlobDtos.Select(x => x.UserPublicId).ToList();
            var users = await _userRepository.GetUsersByPublicId(usersIds, cancellationToken);
        
            foreach (var roomBlobDto in request.ChatRoomDto.ChatRoomKeyBlobDtos)
            {
                var user = users.FirstOrDefault(x=>x.PublicId == roomBlobDto.UserPublicId);
                if (user == null) continue;
            
                var userIdentities = await _userPublicKeyRepository.GetUserIdentitiesByUserId(user.Id, cancellationToken);
                var deviceId = roomBlobDto.DeviceId ?? "Default";
                if (!userIdentities.Any(i => i.DeviceId == deviceId))
                {
                    throw new BadRequestException($"Invalid DeviceId for user {roomBlobDto.UserPublicId}.");
                }

                var existingBlobs = chatRoom.ChatRoomKeyBlobs.Where(x => x.UserId == user.Id).ToList();
                
                foreach(var existingBlob in existingBlobs)
                {
                    chatRoom.ChatRoomKeyBlobs.Remove(existingBlob);
                }

                var newBlob = new ChatRoomKeyBlob
                {
                    ChatRoomId = chatRoom.Id,
                    UserId = user.Id,
                    EncryptedRoomKey = roomBlobDto.EncryptedRoomKey,
                    DeviceId = roomBlobDto.DeviceId ?? "Default",
                    Version = 1,
                    CreatedAtUtc = DateTime.UtcNow
                };
                
                chatRoom.ChatRoomKeyBlobs.Add(newBlob);
            }
        
            chatRoom.IsInitialized = true;

            await _unitOfWork.Commit(cancellationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
        
        return Unit.Value;
    }
}