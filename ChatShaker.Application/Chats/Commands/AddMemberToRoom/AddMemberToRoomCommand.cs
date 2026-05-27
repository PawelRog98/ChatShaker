using ChatShaker.Application.Interfaces;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;

using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Chats.Commands.AddMemberToRoom;

public class AddMemberToRoomCommand : IRequest<Unit>
{
    public AddMemberToRoomCommand(AddMemberToRoomDto addMemberToRoomDto, long hostId)
    {
        AddMemberToRoomDto = addMemberToRoomDto;
        HostId = hostId;
    }

    public AddMemberToRoomDto AddMemberToRoomDto { get; set; }
    public long HostId { get; set; }
}

public class AddMemeberToRoomCommandHandler : IRequestHandler<AddMemberToRoomCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomKeyBlobRepository _chatRoomKeyBlobRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;
    private readonly IUserPublicKeyRepository _userPublicKeyRepository;
    private readonly IChatNotifier _chatNotifier;
    private readonly IUnitOfWork _unitOfWork;
    public AddMemeberToRoomCommandHandler(IUserRepository userRepository,
        IChatRoomRepository chatRoomRepository,
        IChatRoomKeyBlobRepository chatRoomKeyBlobRepository,
        IChatRoomMembershipRepository chatRoomMembershipRepository,
        IUserPublicKeyRepository userPublicKeyRepository,
        IChatNotifier chatNotifier,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _chatRoomRepository = chatRoomRepository;
        _chatRoomKeyBlobRepository = chatRoomKeyBlobRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
        _userPublicKeyRepository = userPublicKeyRepository;
        _chatNotifier = chatNotifier;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddMemberToRoomCommand request, CancellationToken cancellationToken)
    {
        try
        {
            Console.WriteLine("Handler - AddMember: "+ request.AddMemberToRoomDto.RoomPublicId);
            await _unitOfWork.BeginTransaction(cancellationToken);

            var room = await _chatRoomRepository.GetByPublicId(request.AddMemberToRoomDto.RoomPublicId, cancellationToken);

            if (room == null)
                throw new BadRequestException("Room doesn't exists");

            var isMember = await _chatRoomMembershipRepository.Exists(room.Id, request.HostId, cancellationToken);
            if (!isMember)
                throw new ForbiddenException("You are not a member of this room");

            if (room.TypeEnum == Domain.Enums.ChatRoomType.Group && room.HostId != request.HostId)
            {
                throw new ForbiddenException("Only the room host can add new members.");
            }

            var host = await _userRepository.GetUserById(request.HostId, cancellationToken);

            if (host == null)
                throw new BadAuthenticationException("Host not found");

            var userToAdd = await _userRepository.GetUserByPublicId(request.AddMemberToRoomDto.UserToAddPublicId, cancellationToken);

            if (userToAdd == null)
                throw new BadRequestException("User doesn't exists");

            var isUserAlreadyMember = await _chatRoomMembershipRepository.Exists(room.Id, userToAdd.Id, cancellationToken);

            if (room.TypeEnum == Domain.Enums.ChatRoomType.Direct && !isUserAlreadyMember)
            {
                var currentMembers = await _chatRoomMembershipRepository.GetByRoomId(room.Id, cancellationToken);
                if (currentMembers.Count() >= 2)
                {
                    throw new BadRequestException("Direct chat cannot have more than 2 members.");
                }
            }

            var userIdentities = await _userPublicKeyRepository.GetUserIdentitiesByUserId(userToAdd.Id, cancellationToken);
            if (!userIdentities.Any(i => i.DeviceId == request.AddMemberToRoomDto.DeviceId))
            {
                throw new BadRequestException("Invalid DeviceId for the user.");
            }

            var chatBlob = new ChatRoomKeyBlob
            {
                ChatRoomId = room.Id,
                UserId = userToAdd.Id,
                EncryptedRoomKey = request.AddMemberToRoomDto.EncryptedKey,
                DeviceId = request.AddMemberToRoomDto.DeviceId,
                CreatedAtUtc = DateTime.UtcNow,
                Version = await _chatRoomRepository.GetNewestRoomVersion(room.PublicId, cancellationToken)
            };

            await _chatRoomKeyBlobRepository.Add(chatBlob, cancellationToken);

            if (!isUserAlreadyMember)
            {
                var chatMembership = new ChatRoomMembership
                {
                    ChatRoomId = room.Id,
                    UserId = userToAdd.Id,
                    AddedById = host.Id
                };

                await _chatRoomMembershipRepository.Add(chatMembership, cancellationToken);
            }

            await _unitOfWork.Commit(cancellationToken);

            if (!isUserAlreadyMember)
            {
                await _chatNotifier.UserAdded(room.PublicId, userToAdd.PublicId, cancellationToken);
            }

            return Unit.Value;
        }
        catch
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}