using AutoMapper;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Friendships.Query.GetRecievedUserRequests;

public class GetRecievedUserRequestsQuery : IRequest<List<UserRequestsDto>>
{
    public GetRecievedUserRequestsQuery(long userId)
    {
        UserId = userId;
    }
    
    public long UserId { get; set; }
}

public class GetRecievedUserRequestsQueryHandler : IRequestHandler<GetRecievedUserRequestsQuery, List<UserRequestsDto>>
{
    private readonly IFriendRequestRepository _friendRequestRepository;
    private readonly IMapper _mapper;

    public GetRecievedUserRequestsQueryHandler(IFriendRequestRepository friendRequestRepository, IMapper mapper)
    {
        _friendRequestRepository = friendRequestRepository;
        _mapper = mapper;
    }

    public async Task<List<UserRequestsDto>> Handle(GetRecievedUserRequestsQuery request, CancellationToken cancellationToken)
    {
        var data = await _friendRequestRepository.GetRecievedFriendRequests(request.UserId, cancellationToken);

        var requests = _mapper.Map<List<UserRequestsDto>>(data);
        return requests;
    }
}