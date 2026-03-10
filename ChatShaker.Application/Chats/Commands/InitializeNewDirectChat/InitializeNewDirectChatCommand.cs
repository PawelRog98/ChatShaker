using ChatShaker.Domain.Entities;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Chats.Commands.InitializeNewDirectChat;

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
        
            var users = await _userRepository.GetUsersByPublicId(request.ChatRoomDto.ChatRoomKeyBlobDtos.Select(x=>x.UserPublicId).ToList(), cancellationToken);
        
            var chatRoom = await _chatRoomRepository.GetByPublicId(request.ChatRoomDto.ChatRoomPublicId, cancellationToken);
        
            foreach (var roomBlobDto in request.ChatRoomDto.ChatRoomKeyBlobDtos)
            {
                var user = users.FirstOrDefault(x=>x.PublicId == roomBlobDto.UserPublicId);
            
                var chatBlob = chatRoom.ChatRoomKeyBlobs.FirstOrDefault(x=>x.UserId == user.Id);
                chatBlob.EncryptedRoomKey = roomBlobDto.EncryptedRoomKey;
            
            }
        
            chatRoom.IsInitialized = true;

            await _unitOfWork.Commit(cancellationToken);
        }
        catch (Exception e)
        {
            await _unitOfWork.Rollback(cancellationToken);
            throw;
        }
        
        return Unit.Value;
    }
}