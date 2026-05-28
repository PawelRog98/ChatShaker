using AutoMapper;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Chats.Queries.GetRoom;

public class GetRoomQuery : IRequest<RoomDto>
{
    public GetRoomQuery(Guid roomPublicId, long userId)
    {
        RoomPublicId = roomPublicId;
        UserId = userId;
    }
    
    public Guid RoomPublicId { get; set; }
    public long UserId { get; set; }
}

public class GetRoomQueryHandler : IRequestHandler<GetRoomQuery, RoomDto>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomKeyBlobRepository _keyBlobRepository;
    private readonly IMapper _mapper;
    
    public GetRoomQueryHandler(IChatRoomRepository chatRoomRepository,  IChatRoomKeyBlobRepository keyBlobRepository,  IMapper mapper)
    {
        _chatRoomRepository = chatRoomRepository;
        _keyBlobRepository = keyBlobRepository;
        _mapper = mapper;
    }
    
    public async Task<RoomDto> Handle(GetRoomQuery request, CancellationToken cancellationToken)
    {
        var room = await _chatRoomRepository.GetByPublicId(request.RoomPublicId, cancellationToken);
        
        if(room == null)
            throw new BadRequestException("Room not found");

        var newestVersion = await _chatRoomRepository.GetNewestRoomVersion(request.RoomPublicId, cancellationToken);

        var isParticipant = await _keyBlobRepository.GetInfoIsUserHasActiveKey(room.Id, request.UserId, newestVersion, cancellationToken);
        
        if (!isParticipant)
            throw new ForbiddenException("User is not a participant");

        var roomDto = _mapper.Map<RoomDto>(room);
        return roomDto;
    }
}
