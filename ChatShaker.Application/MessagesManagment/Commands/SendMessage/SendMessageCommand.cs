using ChatShaker.Application.Interfaces;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.MessagesManagment.Commands.SendMessage;

public class SendMessageCommand : IRequest<Unit>
{
    public SendMessageCommand(SendMessageDto sendMessageDto, long userId)
    {
        SendMessageDto = sendMessageDto;
        UserId = userId;
    }

    public SendMessageDto SendMessageDto { get; set; }
    public long UserId { get; set; }
}

public class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IChatNotifier _chatNotifier;
    private readonly IUnitOfWork _unitOfWork;
    public SendMessageCommandHandler(IUserRepository userRepository,
        IChatRoomRepository chatRoomRepository,
        IMessageRepository messageRepository,
        IChatNotifier chatNotifier,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _chatRoomRepository = chatRoomRepository;
        _messageRepository = messageRepository;
        _chatNotifier = chatNotifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(SendMessageCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransaction(cancellationToken);

        var room = await _chatRoomRepository.GetByPublicId(request.SendMessageDto.ChatRoomPublicId, cancellationToken);
        if (room == null)
            throw new BadRequestException("Room not found");

        var message = new Message
        {
            ChatRoomId = room.Id,
            SenderId = request.UserId,
            CipherText = request.SendMessageDto.CipherText,
            Nonce = request.SendMessageDto.Nonce,
            SentAtUtc = DateTime.UtcNow,
            ClientMessageId = request.SendMessageDto.ClientMessageId,
            MessageType = request.SendMessageDto.MessageType
        };

        await _messageRepository.Add(message, cancellationToken);

        var messageStatus = new MessageStatus
        {
            Message = message,
            UserId = request.UserId,
            Status = Domain.Enums.MessageStatusEnum.Sent,
            UpdateAtUtc = DateTime.UtcNow
        };

        await _messageRepository.AddStatus(messageStatus, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        var sender = await _userRepository.GetUserById(request.UserId, cancellationToken);
        if (sender == null)
            throw new BadAuthenticationException("User not found");

        var messageDto = new MessageDto
        {
            PublicId = message.PublicId,
            SenderPublicId = sender.PublicId,
            SenderName = sender.FirstName,
            CipherText = message.CipherText,
            Nonce = message.Nonce,
            SentAtUtc = message.SentAtUtc,
            ClientMessageId = message.ClientMessageId,
            Status = Domain.Enums.MessageStatusEnum.Sent,
            MessageType = message.MessageType
        };

        await _chatNotifier.MessageSent(room.PublicId, messageDto, cancellationToken);

        return Unit.Value;
    }
}
