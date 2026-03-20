using ChatShaker.Domain.Exceptions;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Keys.GetRoomKey;

public class GetRoomKeyQuery : IRequest<string>
{
    public GetRoomKeyQuery(long userId, RoomKeyReqestDto roomKeyRequest)
    {
        UserId = userId;
        RoomKeyRequest = roomKeyRequest;
    }
    
    public long UserId  { get; set; }
    public RoomKeyReqestDto RoomKeyRequest { get; set; }
}

public class GetRoomKeyQueryHandler : IRequestHandler<GetRoomKeyQuery, string>
{
    private readonly IChatRoomKeyBlobRepository _chatRoomKeyBlobRepository;
    
    public GetRoomKeyQueryHandler(IChatRoomKeyBlobRepository chatRoomKeyBlobRepository)
    {
        _chatRoomKeyBlobRepository = chatRoomKeyBlobRepository;
    }


    public async Task<string> Handle(GetRoomKeyQuery request, CancellationToken cancellationToken)
    {
        var key = await _chatRoomKeyBlobRepository
            .Get(request.RoomKeyRequest.PublicId,
                request.UserId,
                request.RoomKeyRequest.Version,
                request.RoomKeyRequest.DeviceId,
                cancellationToken);

        if (key == null)
            throw new BadRequestException("Room key not found");

        return key.EncryptedRoomKey;
    }
}