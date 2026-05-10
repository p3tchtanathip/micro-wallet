using API.Attributes;
using Application.Common.Responses;
using Application.Wallets.Commands.Create;
using Application.Wallets.Commands.Deposit;
using Application.Wallets.Commands.Transfer;
using Application.Wallets.Commands.Withdraw;
using Application.Wallets.Queries.GetBalanceSummary;
using Application.Wallets.Queries.GetTransactionHistory;
using Application.Wallets.Queries.GetUserWallet;
using Domain.Enums;
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

        [Authorize]
        [Idempotent]
        [HttpPost("transfer")]
        public async Task<ActionResult> Transfer(TransferCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("{walletId}/transactions")]
        public async Task<ActionResult> GetTransactionHistory(
            [FromRoute] long walletId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] TransactionType? transactionType = null)
        {
            var query = new GetTransactionHistoryQuery
            {
                WalletId = walletId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TransactionType = transactionType
            };

            var result = await _mediator.Send(query);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("balances")]
        public async Task<ActionResult<BalanceSummaryResponse>> GetBalanceSummary()
        {
            var result = await _mediator.Send(new GetBalanceSummaryQuery());
            return Ok(result);
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<ActionResult<WalletResponse[]>> GetUserWallets()
        {
            var result = await _mediator.Send(new GetUserWalletQuery());
            return Ok(result);
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        public async Task<ActionResult<WalletResponse>> CreateWallet(CreateCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
