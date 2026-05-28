using AutoMapper;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Friendships.Query;

public class GetUserRequestsQuery : IRequest<List<UserRequestsDto>>
{
    public GetUserRequestsQuery(long userId)
    {
        UserId = userId;
    }
    
    public long UserId { get; }
}

public class GetUserRequestsQueryHandler : IRequestHandler<GetUserRequestsQuery, List<UserRequestsDto>>
{
    private readonly IFriendRequestRepository _friendRequestRepository;
    private readonly IMapper _mapper;
    
    public GetUserRequestsQueryHandler(IFriendRequestRepository friendRequestRepository,  IMapper mapper)
    {
        _friendRequestRepository = friendRequestRepository;
        _mapper = mapper;
    }
    
    public async Task<List<UserRequestsDto>> Handle(GetUserRequestsQuery request, CancellationToken cancellationToken)
    {
        var data = await _friendRequestRepository.GetSentFriendRequests(request.UserId, cancellationToken);

        var requests = _mapper.Map<List<UserRequestsDto>>(data);
        return requests;
    }
}