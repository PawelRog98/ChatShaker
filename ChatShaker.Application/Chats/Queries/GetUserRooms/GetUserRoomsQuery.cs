using AutoMapper;
using ChatShaker.Domain.Enums;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Chats.Queries.GetUserRooms;

public class GetUserRoomsQuery : IRequest<List<ChatListItemDto>>
{
    public GetUserRoomsQuery(long userId)
    {
        UserId = userId;
    }

    public long UserId { get; set; }

    public class GetUserRoomsQueryHandler : IRequestHandler<GetUserRoomsQuery, List<ChatListItemDto>>
    {
        private readonly IChatRoomRepository _chatRoomRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public GetUserRoomsQueryHandler(IChatRoomRepository chatRoomRepository, IMessageRepository messageRepository,
            IMapper mapper)
        {
            _chatRoomRepository = chatRoomRepository;
            _messageRepository = messageRepository;
            _mapper = mapper;
        }

        public async Task<List<ChatListItemDto>> Handle(GetUserRoomsQuery request, CancellationToken cancellationToken)
        {
            var rooms = await _chatRoomRepository.GetUserRooms(request.UserId, cancellationToken);
            var roomsIds = rooms.Select(x => x.Id).ToArray();

            var lastMessages = await _messageRepository.GetLastMessageByRoom(roomsIds, cancellationToken);

            var roomDtos = new List<ChatListItemDto>();
            foreach (var room in rooms)
            {
                var message = lastMessages.FirstOrDefault(x => x.ChatRoomId == room.Id);
                
                var isRead = true;
                if (message != null)
                {
                    if (message.SenderId == request.UserId)
                    {
                        isRead = true;
                    }
                    else
                    {
                        isRead = message.MessageStatuses
                            .Any(x => x.UserId == request.UserId && x.Status == Domain.Enums.MessageStatusEnum.Read);
                    }
                }

                var roomDto = new ChatListItemDto
                {
                    RoomPublicId = room.PublicId,
                    LastMessagePreview = message != null ? message.CipherText : "",
                    LastMessageNonce = message != null ? message.Nonce : "",
                    Type = message != null ? message.MessageType : Domain.Enums.MessageTypeEnum.Text,
                    LastMessageDate = message != null ? message.SentAtUtc : DateTime.UtcNow,
                    Name = room.TypeEnum == ChatRoomType.Group ? (room.Name ?? "Group Chat") : (room.ChatRoomMemberships.FirstOrDefault(x => x.UserId != request.UserId)?.User?.PublicNick ?? "Unknown"),
                    IsRead = isRead,
                    KeyVersion = message.KeyVersion
                };

                roomDtos.Add(roomDto);
            }

            return roomDtos;
        }
    }
}