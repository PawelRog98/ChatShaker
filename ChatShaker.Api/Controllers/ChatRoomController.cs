using System.Security.Claims;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Chats.Commands.AddMemberToRoom;
using ChatShaker.Application.Chats.CreateChatRoom.Commands;
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

    [HttpPost("create-room")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateChatRoomDto createChatRoomDto, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateChatRoomCommand(createChatRoomDto), cancellationToken);

        return ApiResponse.Ok(result);
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
}
