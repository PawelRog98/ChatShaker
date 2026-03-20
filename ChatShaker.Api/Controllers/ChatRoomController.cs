using System.Security.Claims;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Chats.Commands.AddMemberToRoom;
using ChatShaker.Application.Chats.Commands.InitializeNewDirectChat;
using ChatShaker.Application.Chats.CreateChatRoom.Commands;
using ChatShaker.Application.Chats.GetNewestRoomVersion;
using ChatShaker.Application.Chats.Queries.CheckIfRoomIsInitialized;
using ChatShaker.Application.Chats.Queries.GetUserRooms;
using ChatShaker.Application.MessagesManagment.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatShaker.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatRoomController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatRoomController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("add-member")]
    public async Task<IActionResult> Addmember([FromBody] AddMemberToRoomDto addMemberToRoomDto, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.BadRequest("User not found");

        var result = await _mediator.Send(new AddMemberToRoomCommand(addMemberToRoomDto, long.Parse(userId)), cancellationToken);

        return ApiResponse.Ok();
    }

    [HttpGet("get-all")]
    public async Task<IActionResult> GetRooms(CancellationToken  cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        if(string.IsNullOrWhiteSpace(userId))
            return ApiResponse.BadRequest("User not found");

        var result = await _mediator.Send(new GetUserRoomsQuery(Int64.Parse(userId)),  cancellationToken);
        
        return ApiResponse.Ok(result);
    }

    [HttpGet("{roomPublicId:guid}/messages")]
    public async Task<IActionResult> GetHistoryMessages(Guid roomPublicId, [FromQuery] int pageIndex, [FromQuery] int pageSize)
    {
        var result = await _mediator.Send(new GetMessagesHistoryQuery(roomPublicId, pageIndex, pageSize));
        
        return ApiResponse.Ok(result);
    }

    [HttpPut("initialize-chat")]
    public async Task<IActionResult> InitializeChat([FromBody] ChatRoomDto chatRoomDto,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new InitializeNewDirectChatCommand(chatRoomDto), cancellationToken);
        
        return ApiResponse.Ok(result);
    }

    [HttpGet("initialization-status/{publicId}")]
    [ProducesResponseType(typeof(Response<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInfoIfInitialized(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CheckIfRoomIsInitializedQuery(publicId), cancellationToken);
        
        return ApiResponse.Ok(result);
    }

    [HttpGet("key-version/{publicId}")]
    [ProducesResponseType(typeof(Response<long>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetKeyVersion(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetNewestRoomVersionQuery(publicId), cancellationToken);
        
        return ApiResponse.Ok(result);
    }
}
