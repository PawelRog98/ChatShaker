using ChatShaker.Api.Helpers;
using ChatShaker.Application.Users.Commands.Login;
using ChatShaker.Application.Users.Commands.Refresh;
using ChatShaker.Application.Users.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChatShaker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LoginCommand(loginDto), cancellationToken);

            return ApiResponse.Ok(result, "Token recieved");
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RegisterCommand(registerDto), cancellationToken);

            return ApiResponse.Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken,  CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RefreshCommand(refreshToken), cancellationToken);

            return ApiResponse.Ok(result);
        }
    }
}
