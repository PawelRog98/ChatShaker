using AutoMapper;
using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Keys.Commands.SaveNewRotation;

public class SaveNewRotationCommand : IRequest<Unit>
{
    public SaveNewRotationCommand(Guid roomPublicId, List<RotationDto> rotations)
    {
        RoomPublicId = roomPublicId;
        Rotations = rotations;
    }
    
    public Guid RoomPublicId { get; }
    public List<RotationDto> Rotations { get;}
}

public class SaveNewRotationCommandHandler : IRequestHandler<SaveNewRotationCommand, Unit>
{
    private readonly IChatRoomKeyBlobRepository _chatRoomKeyBlobRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    
    private readonly IMapper _mapper;
    
    public SaveNewRotationCommandHandler(IChatRoomKeyBlobRepository chatRoomKeyBlobRepository,
        IUserRepository userRepository, 
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {  
        _chatRoomKeyBlobRepository = chatRoomKeyBlobRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Unit> Handle(SaveNewRotationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);
            
            var users = await _userRepository.GetUsersByPublicId(request.Rotations.Select(x => x.UserId).ToList(),
                cancellationToken);
            var newKeys = new List<ChatRoomKeyBlob>();

            foreach (var newKey in request.Rotations)
            {
                var currentUser = users.FirstOrDefault(x => x.PublicId == newKey.UserId);

                newKeys.Add(new ChatRoomKeyBlob
                {
                    UserId = currentUser.Id,
                    EncryptedRoomKey = newKey.EncryptedRoomKey,
                    Version = newKey.Version,
                    DeviceId = newKey.DeviceId
                });
            }
            
            await _chatRoomKeyBlobRepository.AddRange(newKeys, cancellationToken);
            await _unitOfWork.Commit(cancellationToken);
            
            return Unit.Value;
        }
        catch (Exception ex)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
    }
}