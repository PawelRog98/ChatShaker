using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Enums;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Friendships.Commands.AcceptInvitation;

public class AcceptInvitationCommand : IRequest<Unit>
{
    public AcceptInvitationCommand(AcceptanceDecisionDto acceptanceDecisionDto, long userId)
    {
        AcceptanceDecisionDto = acceptanceDecisionDto;
        UserId = userId;
    }
    
    public AcceptanceDecisionDto AcceptanceDecisionDto { get; }
    public long UserId { get; }
}

public class AcceptInvitationCommandHandler : IRequestHandler<AcceptInvitationCommand, Unit>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IFriendRequestRepository _friendRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IChatRoomKeyBlobRepository _chatRoomKeyBlobRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUserPublicKeyRepository _userPublicKeyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptInvitationCommandHandler(IFriendshipRepository friendshipRepository, 
        IFriendRequestRepository friendRequestRepository, 
        IUserRepository userRepository, 
        IChatRoomKeyBlobRepository chatRoomKeyBlobRepository,
        IChatRoomMembershipRepository chatRoomMembershipRepository,
        IChatRoomRepository chatRoomRepository,
        IUserPublicKeyRepository userPublicKeyRepository,
        IUnitOfWork unitOfWork)
    {
        _friendshipRepository  = friendshipRepository;
        _friendRequestRepository = friendRequestRepository;
        _userRepository = userRepository;
        _chatRoomKeyBlobRepository = chatRoomKeyBlobRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
        _chatRoomRepository = chatRoomRepository;
        _userPublicKeyRepository = userPublicKeyRepository;
        _unitOfWork = unitOfWork;
    }    public async Task<Unit> Handle(AcceptInvitationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);
            
            var friendRequest = await _friendRequestRepository.FindByPublicId(request.AcceptanceDecisionDto.InvitationRequestId,  cancellationToken);
            
            if(friendRequest == null)
                throw new BadRequestException("Invitation not found");
            
            var recipient = await _userRepository.GetUserById(friendRequest.RecipientId, cancellationToken);
            
            if(recipient == null || recipient.Id != request.UserId)
                throw new BadAuthenticationException("You cannot accept invitation to other user");

            if (!request.AcceptanceDecisionDto.IsAccepted)
            {
                friendRequest.Status = FriendRequestStatus.Declined;
                await _unitOfWork.Commit(cancellationToken);
                
                return Unit.Value;
            }
            
            friendRequest.Status = FriendRequestStatus.Accepted;
            
            var sender = await _userRepository.GetUserById(friendRequest.SenderId, cancellationToken);

            if (sender == null)
                throw new BadRequestException("Sender not found");
            
            var user1Id = Math.Min(recipient.Id, sender.Id);
            var user2Id = Math.Max(recipient.Id, sender.Id);

            var friendship = new Friendship
            {
                User1Id = user1Id,
                User2Id = user2Id,
                CreatedAtUtc = DateTime.UtcNow
            };
            
            await _friendshipRepository.Add(friendship, cancellationToken);

            var chatRoom = new ChatRoom
            {
                HostId = sender.Id,
                Name = $"Private Chat Room {sender.FirstName} - {recipient.FirstName} ",
                CreatedAtUtc = DateTime.UtcNow,
                IsInitialized = false,
                TypeEnum = ChatRoomType.Direct
            };
            
            await _chatRoomRepository.Add(chatRoom, cancellationToken);

            var chatRoomMemberships = new List<ChatRoomMembership>();
            var chatRoomKeyBlobs = new List<ChatRoomKeyBlob>();
            chatRoomMemberships.AddRange(new List<ChatRoomMembership>
            {
                new ChatRoomMembership
                {
                    ChatRoom = chatRoom,
                    UserId = friendRequest.SenderId,
                    AddedById = friendRequest.SenderId
                },
                new ChatRoomMembership
                {
                    ChatRoom = chatRoom,
                    UserId = friendRequest.RecipientId,
                    AddedById = friendRequest.SenderId
                }
            });

            var userPublicKeys = await _userPublicKeyRepository.GetUserIdentities(
                new List<Guid> { sender.PublicId, recipient.PublicId }, cancellationToken);

            if (userPublicKeys.Any())
            {
                foreach (var pubKey in userPublicKeys)
                {
                    chatRoomKeyBlobs.Add(new ChatRoomKeyBlob
                    {
                        ChatRoom = chatRoom,
                        UserId = pubKey.UserId,
                        CreatedAtUtc = DateTime.UtcNow,
                        EncryptedRoomKey = "Placeholder",
                        Version = 0,
                        DeviceId = pubKey.DeviceId
                    });
                }
            }
            else
            {
                chatRoomKeyBlobs.AddRange(new List<ChatRoomKeyBlob>
                {
                    new ChatRoomKeyBlob
                    {
                        ChatRoom = chatRoom,
                        UserId = friendRequest.SenderId,
                        CreatedAtUtc = DateTime.UtcNow,
                        EncryptedRoomKey = "Placeholder",
                        Version = 0,
                        DeviceId = "placeholder"
                    },
                    new ChatRoomKeyBlob
                    {
                        ChatRoom = chatRoom,
                        UserId = friendRequest.RecipientId,
                        CreatedAtUtc = DateTime.UtcNow,
                        EncryptedRoomKey = "Placeholder",
                        Version = 0,
                        DeviceId = "placeholder"
                    }
                });
            }

            foreach (var userMemberships in chatRoomMemberships)
                await _chatRoomMembershipRepository.Add(userMemberships, cancellationToken);

            foreach (var chatRoomKeyBlob in chatRoomKeyBlobs)
                await _chatRoomKeyBlobRepository.Add(chatRoomKeyBlob, cancellationToken);
            
            await _unitOfWork.Commit(cancellationToken);
            return Unit.Value;

        }
        catch(Exception)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}