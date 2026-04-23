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
        var version = await _chatRoomRepository.GetNewestRoomVersion(request.RoomPublicId, cancellationToken);
        
        if(version == null)
            throw new BadRequestException("Room not found");

        var isParticipant = await _keyBlobRepository.GetInfoIsUserHasActiveKey(request.UserId, version, cancellationToken);
        
        if (!isParticipant)
            throw new BadAuthenticationException("User is not a participant");

        var room = await _chatRoomRepository.GetByPublicId(request.RoomPublicId, cancellationToken);
        
        var roomDto = _mapper.Map<RoomDto>(room);
        return roomDto;
    }
}
