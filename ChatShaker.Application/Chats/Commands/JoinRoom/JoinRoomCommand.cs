using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Chats.Commands.JoinRoom;

public class JoinRoomCommand : IRequest<Unit>
{
    public JoinRoomCommand(Guid roomPublicId, long userId)
    {
        RoomPublicId = roomPublicId;
        UserId = userId;
    }

    public Guid RoomPublicId {get; }
    public long UserId {get; }
}

public class JoinRoomCommandHandler : IRequestHandler<JoinRoomCommand, Unit>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IChatRoomMembershipRepository _chatRoomMembershipRepository;
    private readonly IUnitOfWork _unitOfWork;

    public JoinRoomCommandHandler(IChatRoomRepository chatRoomRepository, IChatRoomMembershipRepository chatRoomMembershipRepository, IUnitOfWork unitOfWork)
    {
        _chatRoomRepository = chatRoomRepository;
        _chatRoomMembershipRepository = chatRoomMembershipRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(JoinRoomCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);
            var room = await _chatRoomRepository.GetByPublicId(request.RoomPublicId, cancellationToken)
                ?? throw new BadRequestException("Not found that room");

            var isMember = await _chatRoomMembershipRepository.Exists(room.Id, request.UserId, cancellationToken);

            if (!isMember)
                throw new BadAuthenticationException("User is not a member of this room");

            await _unitOfWork.Commit(cancellationToken);

            return Unit.Value;
        }
        catch (Exception)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}
