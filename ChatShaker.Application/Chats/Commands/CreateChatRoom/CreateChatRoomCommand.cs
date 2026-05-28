using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Enums;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using ChatShaker.Domain.Services;
using MediatR;

namespace ChatShaker.Application.Chats.CreateChatRoom.Commands;

public class CreateChatRoomCommand : IRequest<Guid>
{
    public CreateChatRoomCommand(CreateChatRoomDto chatRoomDto, long userId)
    {
        ChatRoomDto = chatRoomDto;
        UserId = userId;
    }

    public CreateChatRoomDto ChatRoomDto { get; set; }
    public long UserId { get; set; }
}

public class CreateChatRoomCommandHandler : IRequestHandler<CreateChatRoomCommand, Guid>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomKeyBlobRepository _chatRoomKeyBlobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;
    private readonly IUserPublicKeyRepository _userPublicKeyRepository;

    public CreateChatRoomCommandHandler(IChatRoomRepository chatRoomRepository,
        IUnitOfWork unitOfWork,
        IChatRoomKeyBlobRepository chatRoomKeyBlobRepository,
        IUserRepository userRepository,
        IChatRoomMembershipRepository chatRoomMembershipRepository,
        IUserPublicKeyRepository userPublicKeyRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _unitOfWork = unitOfWork;
        _chatRoomKeyBlobRepository = chatRoomKeyBlobRepository;
        _userRepository = userRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
        _userPublicKeyRepository = userPublicKeyRepository;
    }
    public async Task<Guid> Handle(CreateChatRoomCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var members = new List<ChatRoomMembership>();

            await _unitOfWork.BeginTransaction(cancellationToken);

            var host = await _userRepository.GetUserById(request.UserId, cancellationToken);

            var room = new ChatRoom
            {
                Name = request.ChatRoomDto.Name,
                HostId = host.Id,
                CreatedAtUtc = DateTime.UtcNow,
                IsInitialized = true,
                TypeEnum = ChatRoomType.Group
            };

            await _chatRoomRepository.Add(room, cancellationToken);

            await _unitOfWork.SaveChanges(cancellationToken);

            var usersDataDto = request.ChatRoomDto.Keys;
            var usersToAdd = await _userRepository.GetUsersByPublicId(usersDataDto.Select(x => x.UserId).ToList(), cancellationToken);

            foreach(var userData in usersDataDto)
            {
                var userToAdd = usersToAdd.FirstOrDefault(x => x.PublicId == userData.UserId);
                
                var userIdentities = await _userPublicKeyRepository.GetUserIdentitiesByUserId(userToAdd.Id, cancellationToken);
                if (!userIdentities.Any(i => i.DeviceId == userData.DeviceId))
                {
                    throw new BadRequestException($"Invalid DeviceId for user {userData.UserId}.");
                }

                var newBlob = new ChatRoomKeyBlob
                {
                    UserId = userToAdd.Id,
                    ChatRoomId = room.Id,
                    EncryptedRoomKey = userData.EncryptedUserKey,
                    CreatedAtUtc = DateTime.UtcNow,
                    DeviceId = userData.DeviceId ?? "Default",
                    Version = userData.Version == 0 ? 1 : userData.Version
                };

                var newMembership = new ChatRoomMembership
                {
                    UserId = userToAdd.Id,
                    ChatRoomId = room.Id,
                    AddedById = host.Id

                };

                await _chatRoomKeyBlobRepository.Add(newBlob, cancellationToken);
                await _chatRoomMembershipRepository.Add(newMembership, cancellationToken);
            }

            await _unitOfWork.Commit(cancellationToken);

            return room.PublicId;
        }
        catch
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}