using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Enums;
using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Friendships.Commands.SendInvitation;

public class SendInvitationCommand : IRequest<Unit>
{
    public SendInvitationCommand(string invitationCode, long senderId)
    {
        InvitationCode = invitationCode;
        SenderId = senderId;
    }
    
    public string InvitationCode { get; }
    public long SenderId { get; }
}

public class SendInvitationCommandHandler : IRequestHandler<SendInvitationCommand, Unit>
{
    private readonly IFriendRequestRepository _friendRequestRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SendInvitationCommandHandler(IFriendRequestRepository friendRequestRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _friendRequestRepository = friendRequestRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Unit> Handle(SendInvitationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);

            var sender = await _userRepository.GetUserById(request.SenderId, cancellationToken);

            if (sender == null)
                throw new BadRequestException("Sender doesn't exists");

            var recipient = await _userRepository.GetUserByCode(request.InvitationCode, cancellationToken);

            if (recipient == null)
                throw new BadRequestException("User doesn't exists");

            var friendRequest = new FriendRequest
            {
                SenderId = sender.Id,
                RecipientId = recipient.Id,
                CreatedAtUtc = DateTime.UtcNow,
                Status = FriendRequestStatus.Pending
            };

            await _friendRequestRepository.SaveRequest(friendRequest, cancellationToken);
            await _unitOfWork.Commit(cancellationToken);
            return Unit.Value;
        }
        catch
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}