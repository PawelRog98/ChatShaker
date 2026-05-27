using AutoMapper;
using ChatShaker.Application.MessagesManagment.Commands.SendMessage;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.MessagesManagment.Queries;

public class GetMessagesHistoryQuery : IRequest<IEnumerable<MessageDto>>
{
    public GetMessagesHistoryQuery(Guid roomPublicId, long userId, int pageIndex, int pageSize)
    {
        RoomPublicId = roomPublicId;
        UserId = userId;
        PageIndex = pageIndex;
        PageSize = pageSize;
    }
    
    public Guid RoomPublicId { get; set; }
    public long UserId { get; set; }
    public int PageSize { get; set; }
    public int PageIndex { get; set; }
}

public class GetMessagesHistoryQueryHandler : IRequestHandler<GetMessagesHistoryQuery, IEnumerable<MessageDto>>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;
    private readonly IMapper _mapper;

    public GetMessagesHistoryQueryHandler(IMessageRepository messageRepository,
        IChatRoomRepository chatRoomRepository,
        IChatRoomMembershipRepository chatRoomMembershipRepository,
        IMapper mapper)
    {
        _messageRepository = messageRepository;
        _chatRoomRepository = chatRoomRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
        _mapper = mapper;
    }
    
    public async Task<IEnumerable<MessageDto>> Handle(GetMessagesHistoryQuery request, CancellationToken cancellationToken)
    {
        var room = await _chatRoomRepository.GetByPublicId(request.RoomPublicId, cancellationToken)
                   ?? throw new BadRequestException("Not found that room");

        var isMember = await _chatRoomMembershipRepository.Exists(room.Id, request.UserId, cancellationToken);
        if (!isMember)
            throw new ForbiddenException("You are not a member of this room");
        
        var messages =
            await _messageRepository.GetByRoom(room.Id, request.PageIndex, request.PageSize, cancellationToken);
        
        var messagesDto = _mapper.Map<IEnumerable<MessageDto>>(messages).ToList();

        foreach (var messageDto in messagesDto)
        {
            var message = messages.First(x => x.PublicId == messageDto.PublicId);
            
            if (message.SenderId == request.UserId)
            {
                // If I am the sender, I want to see if it was delivered/read by anyone else
                messageDto.Status = message.MessageStatuses
                    .Where(x => x.UserId != request.UserId)
                    .Select(x => x.Status)
                    .DefaultIfEmpty(Domain.Enums.MessageStatusEnum.Sent)
                    .Max();
            }
            else
            {
                // If I am NOT the sender, I want to see MY status for this message
                messageDto.Status = message.MessageStatuses
                    .Where(x => x.UserId == request.UserId)
                    .Select(x => x.Status)
                    .DefaultIfEmpty(Domain.Enums.MessageStatusEnum.Sent)
                    .FirstOrDefault();
            }
        }

        return messagesDto;
    }
}