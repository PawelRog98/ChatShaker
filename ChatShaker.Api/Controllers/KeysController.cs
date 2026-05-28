using System.Security.Claims;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Chats.CreateChatRoom.Commands;
using ChatShaker.Application.Keys.Commands.InitializeNewDirectChat;
using ChatShaker.Application.Keys.Commands.SaveNewRotation;
using ChatShaker.Application.Keys.GetRoomKey;
using ChatShaker.Application.Users.Commands.SaveIdentity;
using ChatShaker.Application.Users.Queries.GetIdentity;
using ChatShaker.Application.Users.Commands.RemoveIdentity;
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
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SaveIdentity([FromBody] UserKeyDataDto userKeyData)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");
        
        await _mediator.Send(new SaveIdentityCommand(userKeyData, long.Parse(userId)));
        
        return ApiResponse.Ok();
    }

    [HttpDelete("remove-identity/{deviceId}")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RemoveIdentity(string deviceId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");

        await _mediator.Send(new RemoveIdentityCommand(deviceId, long.Parse(userId)));

        return ApiResponse.Ok();
    }
    
    [HttpPost("create-room")]
    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRoom([FromBody] CreateChatRoomDto createChatRoomDto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.BadRequest("User not found");
        
        var result = await _mediator.Send(new CreateChatRoomCommand(createChatRoomDto, long.Parse(userId)), cancellationToken);

        return ApiResponse.Ok(result);
    }
    
    [HttpGet("get-public-identities")]
    [ProducesResponseType(typeof(Response<List<UserKeyDataDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPublicIdentities([FromQuery] List<Guid> userIds, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetIdentityQuery(userIds), cancellationToken);
        
        return ApiResponse.Ok(result);
    }

    [HttpPost("get-room-key")]
    [ProducesResponseType(typeof(Response<List<UserKeyDataDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetRoomKey([FromBody] RoomKeyRequestDto roomKeyRequestDto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");

        var result = await _mediator.Send(new GetRoomKeyQuery(long.Parse(userId), roomKeyRequestDto), cancellationToken);
        
        return ApiResponse.Ok(result, "Personal room key");
    }

    [HttpPost("new-room-keys/{publicId}")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RotateKeys(Guid publicId, [FromBody] List<RotationDto> rotateKeysReqestDto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");

        var result = await _mediator.Send(new SaveNewRotationCommand(publicId, rotateKeysReqestDto, long.Parse(userId)), cancellationToken);
        
        return ApiResponse.Ok(result);
    }
    
    [HttpPut("initialize-room-key")]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitializeChat([FromBody] ChatRoomDto chatRoomDto,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.Forbidden("User not found");

        var result = await _mediator.Send(new InitializeNewDirectChatCommand(chatRoomDto, long.Parse(userId)), cancellationToken);
        
        return ApiResponse.Ok(result);
    }
}