using System.Security.Claims;
using ChatShaker.Api.Helpers;
using ChatShaker.Application.Tokens.Commands.ActivateAccount;
using ChatShaker.Application.Tokens.Commands.CreateConfirmationToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatShaker.Api.Controllers;

[Route("api/[controller]")]
public class TokensController : ControllerBase
{
    private readonly IMediator _mediator;

    public TokensController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [ProducesResponseType(typeof(Response<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [HttpPut("activate-account")]
    public async Task<IActionResult> Activate([FromBody] ConfirmAccountDto confirmData, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ConfirmAccountCommand(confirmData), cancellationToken);
        
        return ApiResponse.Ok(result);
    }

    [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [HttpPost("create-new-activation-token")]
    public async Task<IActionResult> Create([FromBody] CreateConfirmationTokenDto createDto ,CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateConfirmationTokenCommand(createDto), cancellationToken);
        
        return ApiResponse.Ok(result);
    }
}