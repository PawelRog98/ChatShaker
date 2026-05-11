using ChatShaker.Application.Interfaces;
using ChatShaker.Application.MessagesManagment.Commands.SendMessage;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChatShaker.UnitTests.Application.Commands;

public class SendMessageCommandHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IChatRoomRepository> _mockChatRoomRepository;
    private readonly Mock<IMessageRepository> _mockMessageRepository;
    private readonly Mock<IChatNotifier> _mockChatNotifier;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly SendMessageCommandHandler _handler;

    public SendMessageCommandHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockChatRoomRepository = new Mock<IChatRoomRepository>();
        _mockMessageRepository = new Mock<IMessageRepository>();
        _mockChatNotifier = new Mock<IChatNotifier>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _handler = new SendMessageCommandHandler(
            _mockUserRepository.Object,
            _mockChatRoomRepository.Object,
            _mockMessageRepository.Object,
            _mockChatNotifier.Object,
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task Handle_SaveMessage_WhenIsValid()
    {
        long userId = 123;
        Guid roomPublicId = Guid.NewGuid();

        var sendMessageDto = new SendMessageDto
        {
            ChatRoomPublicId = roomPublicId,
            CipherText = "Encrypted-text",
            Nonce = "abc",
            ClientMessageId = Guid.NewGuid()
        };

        var chatRoom = new ChatRoom
        {
            Id = 1,
            PublicId = roomPublicId,
            HostId = 1,
            Name = "name",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        var message = new Message
        {
            ChatRoomId = chatRoom.Id,
            SenderId = userId,
            CipherText = sendMessageDto.CipherText,
            Nonce = sendMessageDto.Nonce,
            SentAtUtc = DateTime.UtcNow,
            ClientMessageId = sendMessageDto.ClientMessageId
        };

        var command = new SendMessageCommand(sendMessageDto, userId);

        var sender = new User
        {
            Id = userId,
            PublicId = Guid.NewGuid(),
            FirstName = "SenderName"
        };

        _mockUserRepository
            .Setup(r => r.GetUserById(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(sender);

        _mockChatRoomRepository
            .Setup(r => r.GetByPublicId(roomPublicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatRoom);

        _mockMessageRepository
            .Setup(r => r.Add(message, It.IsAny<CancellationToken>()))
            .Callback<Message, CancellationToken>((msg, _) =>
            {
                msg.Id = 2;
                msg.PublicId = Guid.NewGuid();
            });

        var messageStatus = new MessageStatus
        {
            Message = message,
            UserId = userId,
            Status = Domain.Enums.MessageStatusEnum.Sent,
            UpdateAtUtc = DateTime.UtcNow
        };

        _mockMessageRepository
            .Setup(r => r.SaveStatus(messageStatus, It.IsAny<CancellationToken>()));

        var cancellationToken = new CancellationToken();

        var result = await _handler.Handle(command, cancellationToken);

        result.Should().Be(Unit.Value);

        _mockUnitOfWork.Verify(u => u.BeginTransaction(cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.Commit(cancellationToken), Times.Once);

        _mockChatRoomRepository.Verify(c => c.GetByPublicId(roomPublicId, It.IsAny<CancellationToken>()), Times.Once);
        _mockMessageRepository.Verify(c => c.Add(It.IsAny<Message>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockMessageRepository.Verify(c => c.SaveStatus(It.Is<MessageStatus>(s =>
            s.Status == Domain.Enums.MessageStatusEnum.Sent),
            It.IsAny<CancellationToken>()), Times.Once);

        _mockChatNotifier.Verify(c => c.MessageSent(
            roomPublicId,
            It.Is<MessageDto>(m =>
                m.CipherText == sendMessageDto.CipherText &&
                m.Nonce == sendMessageDto.Nonce),
            cancellationToken),
            Times.Once
        );
        
    }
}
