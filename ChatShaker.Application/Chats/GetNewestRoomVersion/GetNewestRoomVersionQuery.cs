using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Chats.GetNewestRoomVersion;

public class GetNewestRoomVersionQuery : IRequest<long>
{
    public GetNewestRoomVersionQuery(Guid roomPublicId)
    {
        RoomPublicId = roomPublicId;
    }
    
    public Guid RoomPublicId { get; set; }
}

public class GetNewestRoomVersionQueryHandler : IRequestHandler<GetNewestRoomVersionQuery, long>
{
    private readonly IChatRoomRepository _chatRoomRepository;

    public GetNewestRoomVersionQueryHandler(IChatRoomRepository chatRoomRepository)
    {
        _chatRoomRepository = chatRoomRepository;
    }
    
    public async Task<long> Handle(GetNewestRoomVersionQuery request, CancellationToken cancellationToken)
    {
        return await  _chatRoomRepository.GetNewestRoomVersion(request.RoomPublicId, cancellationToken);
    }
}