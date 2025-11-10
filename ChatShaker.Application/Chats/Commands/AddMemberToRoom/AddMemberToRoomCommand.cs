using ChatShaker.Application.Interfaces;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Chats.Commands.AddMemberToRoom;

public class AddMemberToRoomCommand : IRequest<Unit>
{
    public AddMemberToRoomCommand(AddMemberToRoomDto addMemberToRoomDto, long hostId)
    {
        AddMemberToRoomDto = addMemberToRoomDto;
        HostId = hostId;
    }

    public AddMemberToRoomDto AddMemberToRoomDto { get; set; }
    public long HostId { get; set; }
}

public class AddMemeberToRoomCommandHandler : IRequestHandler<AddMemberToRoomCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomKeyBlobRepository _chatRoomKeyBlobRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;
    private readonly IChatNotifier _chatNotifier;
    private readonly IUnitOfWork _unitOfWork;
    public AddMemeberToRoomCommandHandler(IUserRepository userRepository,
        IChatRoomRepository chatRoomRepository,
        IChatRoomKeyBlobRepository chatRoomKeyBlobRepository,
        IChatRoomMembershipRepository chatRoomMembershipRepository,
        IChatNotifier chatNotifier,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _chatRoomRepository = chatRoomRepository;
        _chatRoomKeyBlobRepository = chatRoomKeyBlobRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
        _chatNotifier = chatNotifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddMemberToRoomCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransaction(cancellationToken);

        var room = await _chatRoomRepository.GetByPublicId(request.AddMemberToRoomDto.RoomPublicId, cancellationToken);

        if (room == null)
            throw new BadRequestException("Room doesn't exists");

        var host = await _userRepository.GetUserById(request.HostId, cancellationToken);

        if (host == null)
            throw new BadAuthenticationException("Host not found");

        var userToAdd = await _userRepository.GetUserByPublicId(request.AddMemberToRoomDto.UserToAddPublicId, cancellationToken);

        if (userToAdd == null)
            throw new BadRequestException("User doesn't exists");

        var chatBlob = new ChatRoomKeyBlob
        {
            ChatRoomId = room.Id,
            UserId = userToAdd.Id,
            EncryptedRoomKey = request.AddMemberToRoomDto.EncryptedKey
        };

        await _chatRoomKeyBlobRepository.Add(chatBlob, cancellationToken);

        var chatMembership = new ChatRoomMembership
        {
            ChatRoomId = room.Id,
            UserId = userToAdd.Id,
            AddedById = host.Id
        };

        await _chatRoomMembershipRepository.Add(chatMembership, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        await _chatNotifier.UserAdded(room.PublicId, userToAdd.PublicId, cancellationToken);
        
        return Unit.Value;
    }
}