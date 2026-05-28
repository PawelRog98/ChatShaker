using ChatShaker.Application.Chats.Commands.AddMemberToRoom;
using ChatShaker.Application.Interfaces;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit.Sdk;

namespace ChatShaker.UnitTests.Application.Commands;

public class AddMemberHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IChatRoomRepository> _mockChatRoomRepository;
    private readonly Mock<IChatRoomKeyBlobRepository> _mockChatRoomKeyBlobRepository;
    private readonly Mock<IChatRoomMembershipRepository> _mockChatRoomMembershipRepository;
    private readonly Mock<IUserPublicKeyRepository> _mockUserPublicKeyRepository;
    private readonly Mock<IChatNotifier> _mockChatNotifier;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;

    private readonly AddMemeberToRoomCommandHandler _handler;

    public AddMemberHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockChatRoomRepository = new Mock<IChatRoomRepository>();
        _mockChatRoomKeyBlobRepository = new Mock<IChatRoomKeyBlobRepository>();
        _mockChatRoomMembershipRepository = new Mock<IChatRoomMembershipRepository>();
        _mockUserPublicKeyRepository = new Mock<IUserPublicKeyRepository>();
        _mockChatNotifier = new Mock<IChatNotifier>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _handler = new AddMemeberToRoomCommandHandler(
            _mockUserRepository.Object,
            _mockChatRoomRepository.Object,
            _mockChatRoomKeyBlobRepository.Object,
            _mockChatRoomMembershipRepository.Object,
            _mockUserPublicKeyRepository.Object,
            _mockChatNotifier.Object,
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task Handle_AddNewMember_WhenIsValid()
    {
        var hostPublicId = Guid.NewGuid();
        var userToAddPublicId = Guid.NewGuid();
        var roomPublicId = Guid.NewGuid();

        var addMemberDto = new AddMemberToRoomDto
        {
            UserToAddPublicId = userToAddPublicId,
            RoomPublicId = roomPublicId,
            EncryptedKey = Guid.NewGuid().ToString(),
            DeviceId = "Device1"
        };

        var userToAdd = new User
        {
            Id = 2,
            PublicId = userToAddPublicId,
            PublicNick = "dsfsdfdsdfgdfg",
            FirstName = "dgfdgdsffdg",
            LastName = "Ddfgdasdssfsdfoe",
            DateOfBirth = new DateTime(1990, 5, 15),
            Email = "test2@test.com",
            PasswordHash = "Hashed1",
            IsEmailConfirmed = false,
            LastActivityDateTime = new DateTime(2025, 10, 9, 10, 30, 0, DateTimeKind.Utc),
            AccountInfo = "test",
            CreatedAtUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            ModifiedAtUtc = null
        };

        var host = new User
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

        var room = new ChatRoom
        {
            Id = 1,
            PublicId = roomPublicId,
            HostId = host.Id,
            Name = "TestChatRoom",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1),
            Type = "Group"
        };
        var cancellationToken = new CancellationToken();

        var command = new AddMemberToRoomCommand(addMemberDto, host.Id);

        _mockChatRoomRepository
            .Setup(r => r.GetByPublicId(roomPublicId, cancellationToken))
            .ReturnsAsync(room);

        _mockChatRoomMembershipRepository
            .Setup(r => r.Exists(room.Id, host.Id, cancellationToken))
            .ReturnsAsync(true);

        _mockChatRoomMembershipRepository
            .Setup(r => r.Exists(room.Id, userToAdd.Id, cancellationToken))
            .ReturnsAsync(false);

        _mockUserRepository
            .Setup(r => r.GetUserById(host.Id, cancellationToken))
            .ReturnsAsync(host);

        _mockUserRepository
            .Setup(r => r.GetUserByPublicId(userToAddPublicId, cancellationToken))
            .ReturnsAsync(userToAdd);

        _mockUserPublicKeyRepository
            .Setup(r => r.GetUserIdentitiesByUserId(userToAdd.Id, cancellationToken))
            .ReturnsAsync(new List<UserPublicKey> { new UserPublicKey { DeviceId = "Device1" } });

        _mockChatRoomKeyBlobRepository
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

        act.Should().Be(Unit.Value);

        _mockChatRoomRepository
            .Verify(u => u.GetByPublicId(roomPublicId, cancellationToken), Times.Once);

        _mockUserRepository
            .Verify(u => u.GetUserById(host.Id, cancellationToken), Times.Once);

        _mockUserRepository
            .Verify(u => u.GetUserByPublicId(userToAddPublicId, cancellationToken), Times.Once);

        _mockChatRoomKeyBlobRepository
            .Verify(u => u.Add(It.IsAny<ChatRoomKeyBlob>(), cancellationToken), Times.Once);

        _mockChatRoomMembershipRepository
            .Verify(u => u.Add(It.IsAny<ChatRoomMembership>(), cancellationToken), Times.Once);

        _mockUnitOfWork.Verify(u => u.BeginTransaction(cancellationToken), Times.Once);
        _mockUnitOfWork.Verify(u => u.Commit(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_AddNewMember_WhenRoomIsInvalid()
    {
        var hostPublicId = Guid.NewGuid();
        var userToAddPublicId = Guid.NewGuid();
        var roomPublicId = Guid.NewGuid();

        var addMemberDto = new AddMemberToRoomDto
        {
            UserToAddPublicId = userToAddPublicId,
            RoomPublicId = roomPublicId,
            EncryptedKey = Guid.NewGuid().ToString(),
            DeviceId = "Device1"
        };
        var command = new AddMemberToRoomCommand(addMemberDto, 1);
        var cancellationToken = new CancellationToken();

        _mockChatRoomRepository
            .Setup(r => r.GetByPublicId(roomPublicId, cancellationToken))
            .ReturnsAsync((ChatRoom)null);

        var act = async () => await _handler.Handle(command, cancellationToken);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Room doesn't exists");

        _mockChatRoomRepository
            .Verify(u => u.GetByPublicId(roomPublicId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_AddNewMember_WhenHostIsInvalid()
    {
        var hostPublicId = Guid.NewGuid();
        var userToAddPublicId = Guid.NewGuid();
        var roomPublicId = Guid.NewGuid();

        var addMemberDto = new AddMemberToRoomDto
        {
            UserToAddPublicId = userToAddPublicId,
            RoomPublicId = roomPublicId,
            EncryptedKey = Guid.NewGuid().ToString(),
            DeviceId = "Device1"
        };

        var room = new ChatRoom
        {
            Id = 1,
            PublicId = roomPublicId,
            HostId = 1,
            Name = "TestChatRoom",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        var cancellationToken = new CancellationToken();

        var command = new AddMemberToRoomCommand(addMemberDto, 1);

        _mockChatRoomRepository
            .Setup(r => r.GetByPublicId(roomPublicId, cancellationToken))
            .ReturnsAsync(room);

        _mockChatRoomMembershipRepository
            .Setup(r => r.Exists(room.Id, It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(true);

        _mockUserRepository
            .Setup(r=>r.GetUserById(1, cancellationToken))
            .ReturnsAsync((User)null);

        var act = async () => await _handler.Handle(command, cancellationToken);

        await act.Should().ThrowAsync<BadAuthenticationException>()
            .WithMessage("Host not found");

        _mockChatRoomRepository
            .Verify(u => u.GetByPublicId(roomPublicId, cancellationToken), Times.Once);
        
        _mockUserRepository
            .Verify(u=>u.GetUserById(1, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task Handle_AddNewMember_WhenUserToAddIsInvalid()
    {
        var hostPublicId = Guid.NewGuid();
        var userToAddPublicId = Guid.NewGuid();
        var roomPublicId = Guid.NewGuid();

        var addMemberDto = new AddMemberToRoomDto
        {
            UserToAddPublicId = userToAddPublicId,
            RoomPublicId = roomPublicId,
            EncryptedKey = Guid.NewGuid().ToString(),
            DeviceId = "Device1"
        };

        var host = new User
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

        var room = new ChatRoom
        {
            Id = 1,
            PublicId = roomPublicId,
            HostId = 1,
            Name = "TestChatRoom",
            CreatedAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        var cancellationToken = new CancellationToken();

        var command = new AddMemberToRoomCommand(addMemberDto, 1);

        _mockChatRoomRepository
            .Setup(r => r.GetByPublicId(roomPublicId, cancellationToken))
            .ReturnsAsync(room);

        _mockChatRoomMembershipRepository
            .Setup(r => r.Exists(room.Id, It.IsAny<long>(), cancellationToken))
            .ReturnsAsync(true);

        _mockUserRepository
            .Setup(r=>r.GetUserById(host.Id, cancellationToken))
            .ReturnsAsync(host);

        _mockUserRepository
            .Setup(r=>r.GetUserByPublicId(userToAddPublicId, cancellationToken))
            .ReturnsAsync((User)null);

        var act = async () => await _handler.Handle(command, cancellationToken);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("User doesn't exists");

        _mockChatRoomRepository
            .Verify(u => u.GetByPublicId(roomPublicId, cancellationToken), Times.Once);
        
        _mockUserRepository
            .Verify(u=>u.GetUserById(host.Id, cancellationToken), Times.Once);

        _mockUserRepository
            .Verify(u=>u.GetUserByPublicId(userToAddPublicId, cancellationToken), Times.Once);
    }
}
