using System.Security.Claims;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Users.Queries.GetFriends;
using ChatShaker.Application.Users.Queries.GetIdentity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatShaker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("get-friends")]
    [ProducesResponseType(typeof(Response<List<UserInfoDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFriends(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");
        
        var friends = await _mediator.Send(new GetFriendsQuery(long.Parse(userId)), cancellationToken);
        
        return ApiResponse.Ok(friends);
    }
}