using AutoMapper;
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
            foreach (var message in lastMessages)
            {
                var room = rooms.FirstOrDefault(x => x.Id == message.ChatRoomId);
                var roomDto = new ChatListItemDto
                {
                    RoomPublicId = room.PublicId,
                    LastMessagePreview = message.CipherText,
                    LastMessageDate = message.SentAtUtc,
                    Name = room?.Name
                };

                roomDtos.Add(roomDto);
            }

            return roomDtos;
        }
    }
}