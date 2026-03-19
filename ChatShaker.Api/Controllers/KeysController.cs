using System.Security.Claims;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Keys.GetRoomKey;
using ChatShaker.Application.Users.Commands.SaveIdentity;
using ChatShaker.Application.Users.Queries.GetIdentity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatShaker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class KeysController :  ControllerBase
{
    private readonly IMediator _mediator;

    public KeysController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("save-identity")]
    public async Task<IActionResult> SaveIdentity([FromBody] UserKeyDataDto userKeyData)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");
        
        await _mediator.Send(new SaveIdentityCommand(userKeyData, long.Parse(userId)));
        
        return ApiResponse.Ok();
    }
    
    [HttpGet("get-public-identities")]
    [ProducesResponseType(typeof(Response<List<UserKeyDataDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPublicIdentities([FromQuery] List<Guid> userIds, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetIdentityQuery(userIds), cancellationToken);
        
        return ApiResponse.Ok(result);
    }

    [HttpGet("get-room-key")]
    [ProducesResponseType(typeof(Response<List<UserKeyDataDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRoomKey([FromBody] RoomKeyReqestDto roomKeyReqestDto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");

        var result = await _mediator.Send(new GetRoomKeyQuery(long.Parse(userId), roomKeyReqestDto), cancellationToken);
        
        return ApiResponse.Ok(result, "Personal room key");
    }
}