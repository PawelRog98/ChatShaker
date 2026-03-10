using System.Security.Claims;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Friendships.Commands.AcceptInvitation;
using ChatShaker.Application.Friendships.Commands.SendInvitation;
using ChatShaker.Application.Friendships.Query;
using ChatShaker.Application.Friendships.Query.GetRecievedUserRequests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatShaker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FriendshipController : ControllerBase
{
    private readonly IMediator _mediator;

    public FriendshipController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("send")]
    [ProducesResponseType(typeof(Response<object>) ,StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendRequest([FromQuery] string invitationCode, CancellationToken  cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");

        await _mediator.Send(new SendInvitationCommand(invitationCode, long.Parse(userId)), cancellationToken);
        
        return ApiResponse.Ok();
    }

    [HttpPost("accept-invitation")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AcceptInvitation([FromBody] AcceptanceDecisionDto  acceptanceDecisionDto, CancellationToken  cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");
        
        await _mediator.Send(new AcceptInvitationCommand(acceptanceDecisionDto, long.Parse(userId)), cancellationToken);
        
        return ApiResponse.Ok();
    }

    [HttpGet("get-recieved")]
    [ProducesResponseType(typeof(Response<UserRequestsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRecievedUserRequests(CancellationToken  cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");
        
        var result = await _mediator.Send(new GetRecievedUserRequestsQuery(long.Parse(userId)), cancellationToken);
        
        return ApiResponse.Ok(result);
    }

    [HttpGet("get-sent")]
    [ProducesResponseType(typeof(Response<UserRequestsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSentUserRequests(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");
        
        var result = await _mediator.Send(new GetUserRequestsQuery(long.Parse(userId)), cancellationToken);
        
        return ApiResponse.Ok(result);
    }
}