using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Chats.GetNewestRoomVersion;

public class GetNewestRoomVersionQuery : IRequest<long>
{
    public GetNewestRoomVersionQuery(Guid roomPublicId, long userId)
    {
        RoomPublicId = roomPublicId;
        UserId = userId;
    }
    
    public Guid RoomPublicId { get; set; }
    public long UserId { get; set; }
}

public class GetNewestRoomVersionQueryHandler : IRequestHandler<GetNewestRoomVersionQuery, long>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;

    public GetNewestRoomVersionQueryHandler(IChatRoomRepository chatRoomRepository, IChatRoomMembershipRepository chatRoomMembershipRepository)
    {
        _chatRoomRepository = chatRoomRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
    }
    
    public async Task<long> Handle(GetNewestRoomVersionQuery request, CancellationToken cancellationToken)
    {
        var room = await _chatRoomRepository.GetByPublicId(request.RoomPublicId, cancellationToken);
        if (room == null)
            throw new ChatShaker.Domain.Exceptions.BadRequestException("Room not found");

        var isMember = await _chatRoomMembershipRepository.Exists(room.Id, request.UserId, cancellationToken);
        if (!isMember)
            throw new ChatShaker.Domain.Exceptions.ForbiddenException("You are not a member of this room");

        return await  _chatRoomRepository.GetNewestRoomVersion(request.RoomPublicId, cancellationToken);
    }
}