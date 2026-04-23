using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Keys.Commands.InitializeNewDirectChat;

public class InitializeNewDirectChatCommand : IRequest<Unit>
{
    public InitializeNewDirectChatCommand(ChatRoomDto chatRoomDto)
    {
        ChatRoomDto = chatRoomDto;
    }
    
    public ChatRoomDto ChatRoomDto { get; set; }
}

public class InitializeNewDirecChatCommandHandler : IRequestHandler<InitializeNewDirectChatCommand, Unit>
{
    private readonly IChatRoomRepository _chatRoomRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public InitializeNewDirecChatCommandHandler(IChatRoomRepository chatRoomRepository,  IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _chatRoomRepository = chatRoomRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Unit> Handle(InitializeNewDirectChatCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.BeginTransaction(cancellationToken);

            var usersIds = request.ChatRoomDto.ChatRoomKeyBlobDtos.Select(x => x.UserPublicId).ToList();
            var users = await _userRepository.GetUsersByPublicId(usersIds, cancellationToken);
        
            var chatRoom = await _chatRoomRepository.GetByPublicId(request.ChatRoomDto.ChatRoomPublicId, cancellationToken);
        
            foreach (var roomBlobDto in request.ChatRoomDto.ChatRoomKeyBlobDtos)
            {
                var user = users.FirstOrDefault(x=>x.PublicId == roomBlobDto.UserPublicId);
                if (user == null) continue;
            
                var existingBlobs = chatRoom.ChatRoomKeyBlobs.Where(x => x.UserId == user.Id).ToList();
                
                foreach(var existingBlob in existingBlobs)
                {
                    chatRoom.ChatRoomKeyBlobs.Remove(existingBlob);
                }

                var newBlob = new ChatRoomKeyBlob
                {
                    ChatRoomId = chatRoom.Id,
                    UserId = user.Id,
                    EncryptedRoomKey = roomBlobDto.EncryptedRoomKey,
                    DeviceId = roomBlobDto.DeviceId ?? "Default",
                    Version = 1,
                    CreatedAtUtc = DateTime.UtcNow
                };
                
                chatRoom.ChatRoomKeyBlobs.Add(newBlob);
            }
        
            chatRoom.IsInitialized = true;

            await _unitOfWork.Commit(cancellationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
        
        return Unit.Value;
    }
}