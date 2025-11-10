using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using ChatShaker.Domain.Services;
using MediatR;

namespace ChatShaker.Application.Chats.CreateChatRoom.Commands;

public class CreateChatRoomCommand : IRequest<Guid>
{
    public CreateChatRoomCommand(CreateChatRoomDto chatRoomDto)
    {
        ChatRoomDto = chatRoomDto;
    }

    public CreateChatRoomDto ChatRoomDto { get; set; }
}

public class CreateChatRoomCommandHandler : IRequestHandler<CreateChatRoomCommand, Guid>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomKeyBlobRepository _chatRoomKeyBlobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;

    public CreateChatRoomCommandHandler(IChatRoomRepository chatRoomRepository,
        IUnitOfWork unitOfWork,
        IChatRoomKeyBlobRepository chatRoomKeyBlobRepository,
        IUserRepository userRepository,
        IChatRoomMembershipRepository chatRoomMembershipRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _unitOfWork = unitOfWork;
        _chatRoomKeyBlobRepository = chatRoomKeyBlobRepository;
        _userRepository = userRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
    }
    public async Task<Guid> Handle(CreateChatRoomCommand request, CancellationToken cancellationToken)
    {
        var members = new List<ChatRoomMembership>();

        await _unitOfWork.BeginTransaction(cancellationToken);

        var hostDataDto = request.ChatRoomDto.Users.FirstOrDefault(x => x.isHost == true);
        var host = await _userRepository.GetUserByPublicId(hostDataDto.PublicId, cancellationToken);

        var room = new ChatRoom
        {
            Name = request.ChatRoomDto.Name,
            HostId = host.Id,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _chatRoomRepository.Add(room, cancellationToken);

        var usersDataDto = request.ChatRoomDto.Users;
        var usersToAdd = await _userRepository.GetUsersByPublicId(usersDataDto.Select(x => x.PublicId).ToList(), cancellationToken);

        foreach(var userData in request.ChatRoomDto.Users)
        {
            var userToAdd = usersToAdd.FirstOrDefault(x => x.PublicId == userData.PublicId);
            var newBlob = new ChatRoomKeyBlob
            {
                UserId = userToAdd.Id,
                ChatRoomId = room.Id,
                EncryptedRoomKey = userData.EncryptedUserKey,
                CreatedAtUtc = DateTime.UtcNow
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
}