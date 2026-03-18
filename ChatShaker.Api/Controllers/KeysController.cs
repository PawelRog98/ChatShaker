using System.Security.Claims;
using ChatShaker.Api.Helpers;
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
            return ApiResponse.BadRequest("User not found");
        
        await _mediator.Send(new SaveIdentityCommand(userKeyData, long.Parse(userId)));
        
        return ApiResponse.Ok();
    }
}