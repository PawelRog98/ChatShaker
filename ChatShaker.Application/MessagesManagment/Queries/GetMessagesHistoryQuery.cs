using AutoMapper;
using ChatShaker.Application.MessagesManagment.Commands.SendMessage;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.MessagesManagment.Queries;

public class GetMessagesHistoryQuery : IRequest<IEnumerable<MessageDto>>
{
    public GetMessagesHistoryQuery(Guid roomPublicId, int pageIndex, int pageSize)
    {
        RoomPublicId = roomPublicId;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
    
    public Guid RoomPublicId { get; set; }
    public int PageSize { get; set; }
    public int PageIndex { get; set; }
}

public class GetMessagesHistoryQueryHandler : IRequestHandler<GetMessagesHistoryQuery, IEnumerable<MessageDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IMapper _mapper;

    public GetMessagesHistoryQueryHandler(IMessageRepository messageRepository,  IChatRoomRepository chatRoomRepository,  IMapper mapper)
    {
        _messageRepository = messageRepository;
        _chatRoomRepository = chatRoomRepository;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<MessageDto>> Handle(GetMessagesHistoryQuery request, CancellationToken cancellationToken)
    {
        var room = await _chatRoomRepository.GetByPublicId(request.RoomPublicId, cancellationToken)
                   ?? throw new BadRequestException("Not found that room");;
        
        var messages =
            await _messageRepository.GetByRoom(room.Id, request.PageIndex, request.PageSize, cancellationToken);
        
        var messagesDto = _mapper.Map<IEnumerable<MessageDto>>(messages);
        return messagesDto;
    }
}