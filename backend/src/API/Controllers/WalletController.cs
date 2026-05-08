using API.Attributes;
using Application.Wallets.Commands.Deposit;
using Application.Wallets.Commands.Withdraw;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IMediator _mediator;
        public WalletController(IMediator mediator) => _mediator = mediator;

        [Authorize]
        [Idempotent]
        [HttpPost("deposit")]
        public async Task<ActionResult> Deposit(DepositCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [Idempotent]
        [HttpPost("withdraw")]
        public async Task<ActionResult> Withdraw(WithdrawCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
