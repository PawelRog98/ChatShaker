using ChatShaker.Application.Interfaces;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.MessagesManagment.Commands.MarkMessageAsRead;

public class MarkMessageAsReadCommand : IRequest<Unit>
{
    public MarkMessageAsReadCommand(Guid messagePublicId, long userId)
    {
        MessagePublicId = messagePublicId;
        UserId = userId;
    }

    public Guid MessagePublicId { get; set; }
    public long UserId { get; set; }
}

public class MarkMessageAsReadCommandHandler : IRequestHandler<MarkMessageAsReadCommand, Unit>
{
    private readonly IMessageRepository _messageRepository;
    private readonly IMessageStatusRepository _messageStatusRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatNotifier _chatNotifier;
    private readonly IUnitOfWork _unitOfWork;

    public MarkMessageAsReadCommandHandler(
        IMessageRepository messageRepository,
        IMessageStatusRepository messageStatusRepository,
        IChatRoomRepository chatRoomRepository,
        IChatNotifier chatNotifier,
        IUnitOfWork unitOfWork)
    {
        _messageRepository = messageRepository;
        _messageStatusRepository = messageStatusRepository;
        _chatRoomRepository = chatRoomRepository;
        _chatNotifier = chatNotifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransaction(cancellationToken);

        var messages = await _messageRepository.GetByPublicId(request.MessagePublicId, cancellationToken);
        var message = messages.FirstOrDefault();
        
        if (message == null)
            throw new BadRequestException("Message not found");

        var status = await _messageStatusRepository.GetStatus(message.Id, request.UserId, cancellationToken);

        if (status == null)
        {
            status = new MessageStatus
            {
                MessageId = message.Id,
                UserId = request.UserId,
                Status = Domain.Enums.MessageStatusEnum.Read,
                UpdateAtUtc = DateTime.UtcNow
            };
            await _messageStatusRepository.SetStatus(status, cancellationToken);
        }
        else if (status.Status < Domain.Enums.MessageStatusEnum.Read)
        {
            status.SetAsRead();
            status.UpdateAtUtc = DateTime.UtcNow;
            await _messageStatusRepository.UpdateStatus(status, cancellationToken);
        }
        else
        {
            await _unitOfWork.Rollback(cancellationToken);
            return Unit.Value;
        }

        await _unitOfWork.Commit(cancellationToken);

        var room = await _chatRoomRepository.GetById(message.ChatRoomId, cancellationToken);
        if (room != null)
        {
            await _chatNotifier.MessageRead(room.PublicId, message.PublicId, cancellationToken);
        }

        return Unit.Value;
    }
}
