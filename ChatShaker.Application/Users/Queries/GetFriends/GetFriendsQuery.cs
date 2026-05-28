using AutoMapper;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Users.Queries.GetFriends;

public class GetFriendsQuery : IRequest<List<UserInfoDto>>
{
    public GetFriendsQuery(long userId)
    {
        UserId = userId;
    }
    
    public long UserId  { get; }
}

public class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, List<UserInfoDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetFriendsQueryHandler(IUserRepository userRepository,  IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }
    
    public async Task<List<UserInfoDto>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
    {
        var friendsData = await _userRepository.GetFriends(request.UserId, cancellationToken);
        
        var friends = _mapper.Map<List<UserInfoDto>>(friendsData);
        return friends;
    }
}
