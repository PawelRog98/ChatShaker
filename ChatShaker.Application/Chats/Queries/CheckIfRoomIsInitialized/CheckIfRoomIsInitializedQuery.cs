using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Chats.Queries.CheckIfRoomIsInitialized;

public class CheckIfRoomIsInitializedQuery : IRequest<bool>
{
    public CheckIfRoomIsInitializedQuery(Guid roomPublicId)
    {
        RoomPublicId = roomPublicId;
    }
    
    public Guid RoomPublicId  { get; set; }
}

public class CheckIfRoomIsInitializedQueryHandler : IRequestHandler<CheckIfRoomIsInitializedQuery, bool>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    
    public CheckIfRoomIsInitializedQueryHandler(IChatRoomRepository chatRoomRepository)
    {
        _chatRoomRepository = chatRoomRepository;
    }


    public async Task<bool> Handle(CheckIfRoomIsInitializedQuery request, CancellationToken cancellationToken)
    {
        var room = await _chatRoomRepository.GetByPublicId(request.RoomPublicId, cancellationToken);
        
        if (room == null)
            throw new BadRequestException("Room not found");

        return room.IsInitialized;
    }
}