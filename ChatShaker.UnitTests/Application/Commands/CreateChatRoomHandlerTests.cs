using ChatShaker.Application.Chats.CreateChatRoom.Commands;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Moq;

namespace ChatShaker.UnitTests.Application.Commands;

public class CreateChatRoomHandlerTests
{
    private readonly Mock<IChatRoomRepository> _mockRoomRepository;
    private readonly Mock<IChatRoomKeyBlobRepository> _mockKeyBlobRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IChatRoomMembershipRepository> _mockChatRoomMembershipRepository;
    private readonly CreateChatRoomCommandHandler _handler;

    public CreateChatRoomHandlerTests()
    {
        _mockRoomRepository = new Mock<IChatRoomRepository>();
        _mockKeyBlobRepository = new Mock<IChatRoomKeyBlobRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockChatRoomMembershipRepository = new Mock<IChatRoomMembershipRepository>();
        _handler = new CreateChatRoomCommandHandler(
            _mockRoomRepository.Object,
            _mockUnitOfWork.Object,
            _mockKeyBlobRepository.Object,
            _mockUserRepository.Object,
            _mockChatRoomMembershipRepository.Object);
    }


    [Fact]
    public async Task Handle_CreateChatRoom_WhenValid()
    {
        var hostPublicId = Guid.NewGuid();
        var secondUserPublicId = Guid.NewGuid();

        var usersDtos = new List<UserEncryptionDto>
        {
            new UserEncryptionDto
            {
                UserId = hostPublicId,
                EncryptedUserKey = Guid.NewGuid().ToString(),
                IsHost = true,
                DeviceId = "device1"
            },
            new UserEncryptionDto
            {
                UserId = secondUserPublicId,
                EncryptedUserKey = Guid.NewGuid().ToString(),
                IsHost = false,
                DeviceId = "device2"
            }
        };
        var createChatRoomDto = new CreateChatRoomDto
        {
            Name = "Room1",
            CreatedAtUtc = DateTime.UtcNow,
            Keys = usersDtos
        };

        var hostData = new User
        {
            Id = 1,
            PublicId = hostPublicId,
            PublicNick = "dsfdfgdfg",
            FirstName = "dgfdgfdg",
            LastName = "Ddfgdssfsdfoe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Email = "test@test.com",
            PasswordHash = "Hashed1",
            IsEmailConfirmed = false,
            LastActivityDateTime = new DateTime(2025, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            AccountInfo = "test",
            CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedAtUtc = null
        };
        var secondUserData = new User
        {
            Id = 2,
            PublicId = secondUserPublicId,
            PublicNick = "dsfdasdfgdfg",
            FirstName = "dgfdgasdasfdg",
            LastName = "fsdgfdgdfgdfg",
            DateOfBirth = new DateTime(1990, 5, 15),
            Email = "test2@test.com",
            PasswordHash = "Hashed12",
            IsEmailConfirmed = false,
            LastActivityDateTime = new DateTime(2025, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            AccountInfo = "test",
            CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedAtUtc = null
        };

        var cancellationToken = new CancellationToken();

        var command = new CreateChatRoomCommand(createChatRoomDto, hostData.Id);

        _mockUserRepository
            .Setup(r => r.GetUserById(It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(hostData);

        _mockUserRepository
            .Setup(r => r.GetUsersByPublicId(It.IsAny<List<Guid>>(), cancellationToken))
            .ReturnsAsync(new List<User>
            {
                hostData,
                secondUserData
            });

        _mockRoomRepository
            .Setup(r => r.Add(It.IsAny<ChatRoom>(), cancellationToken))
            .Returns(Task.CompletedTask)
            .Callback<ChatRoom, CancellationToken>((r, _) => r.PublicId = Guid.NewGuid());

        _mockKeyBlobRepository
            .Setup(r => r.Add(It.IsAny<ChatRoomKeyBlob>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _mockChatRoomMembershipRepository
            .Setup(r => r.Add(It.IsAny<ChatRoomMembership>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.BeginTransaction(cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.Commit(cancellationToken))
            .Returns(Task.CompletedTask);

        var act = await _handler.Handle(command, cancellationToken);

        act.Should().NotBeEmpty();

        _mockUserRepository
            .Verify(u => u.GetUserById(It.IsAny<long>(), cancellationToken), Times.Once);
        _mockUserRepository
            .Verify(u => u.GetUsersByPublicId(It.IsAny<List<Guid>>(), cancellationToken), Times.Once);

        _mockKeyBlobRepository
            .Verify(u => u.Add(It.IsAny<ChatRoomKeyBlob>(), cancellationToken), Times.Exactly(2));
        _mockChatRoomMembershipRepository
            .Verify(u => u.Add(It.IsAny<ChatRoomMembership>(), cancellationToken), Times.Exactly(2));

        _mockUnitOfWork.Verify(u => u.BeginTransaction(cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.Commit(cancellationToken), Times.Once);
    }
}
