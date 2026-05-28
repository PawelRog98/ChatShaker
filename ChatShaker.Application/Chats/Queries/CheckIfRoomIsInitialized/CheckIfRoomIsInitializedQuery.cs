using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Chats.Queries.CheckIfRoomIsInitialized;

public class CheckIfRoomIsInitializedQuery : IRequest<bool>
{
    public CheckIfRoomIsInitializedQuery(Guid roomPublicId, long userId)
    {
        RoomPublicId = roomPublicId;
        UserId = userId;
    }

    public Guid RoomPublicId  { get; set; }
    public long UserId { get; set; }
}

public class CheckIfRoomIsInitializedQueryHandler : IRequestHandler<CheckIfRoomIsInitializedQuery, bool>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;

    public CheckIfRoomIsInitializedQueryHandler(IChatRoomRepository chatRoomRepository,
        IChatRoomMembershipRepository chatRoomMembershipRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
    }


    public async Task<bool> Handle(CheckIfRoomIsInitializedQuery request, CancellationToken cancellationToken)
    {
        var room = await _chatRoomRepository.GetByPublicId(request.RoomPublicId, cancellationToken);

        if (room == null)
            throw new BadRequestException("Room not found");

        var isMember = await _chatRoomMembershipRepository.Exists(room.Id, request.UserId, cancellationToken);
        if (!isMember)
            throw new ForbiddenException("You are not a member of this room");

        return room.IsInitialized;
    }
}