using AutoMapper;
using ChatShaker.Application.Users.Commands.SaveIdentity;
using ChatShaker.Domain.Repositories;
using MediatR;

namespace ChatShaker.Application.Users.Queries.GetIdentity;

public class GetIdentityQuery : IRequest<List<UserKeyDataDto>>
{
    public GetIdentityQuery(List<Guid> userIds)
    {
        UserIds = userIds;
    }
    
    public List<Guid> UserIds { get; set; }
}

public class GetIdentityQueryHandler : IRequestHandler<GetIdentityQuery, List<UserKeyDataDto>>
{
    private readonly IUserPublicKeyRepository _userPublicKeyRepository;
    private readonly IMapper _mapper;

    public GetIdentityQueryHandler(IUserPublicKeyRepository userPublicKeyRepository,  IMapper mapper)
    {
        _userPublicKeyRepository = userPublicKeyRepository;
        _mapper = mapper;
    }
    
    public async Task<List<UserKeyDataDto>> Handle(GetIdentityQuery request, CancellationToken cancellationToken)
    {
        var result = await _userPublicKeyRepository.GetUserIdentities(request.UserIds, cancellationToken);
        
        var userKeysDto = _mapper.Map<List<UserKeyDataDto>>(result);
        return userKeysDto;
    }
}