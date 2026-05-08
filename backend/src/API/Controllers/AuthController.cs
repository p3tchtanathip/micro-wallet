using Application.Auth.Commands.GoogleLogin;
using Application.Auth.Commands.Login;
using Application.Auth.Commands.RefreshToken;
using Application.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AuthController(IMediator mediator) => _mediator = mediator;

        [HttpPost("register")]
        public async Task<ActionResult<long>> Register(RegisterCommand command)
        {
            var userId = await _mediator.Send(command);
            return Ok(userId);
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("google-login")]
        public async Task<ActionResult<LoginResponse>> GoogleLogin([FromBody] string idToken)
        {
            var result = await _mediator.Send(new GoogleLoginCommand(idToken));
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<RefreshTokenResponse>> Refresh(RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("debug")]
        public ActionResult Debug()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value });
            return Ok(claims);
        }
    }
}
