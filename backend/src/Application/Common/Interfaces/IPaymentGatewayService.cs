namespace Application.Common.Interfaces;

public interface IPaymentGatewayService
{
    Task<GatewayResult> DepositAsync(decimal amount);

    Task<GatewayResult> WithdrawAsync(decimal amount);
}

public record GatewayResult(bool Success, string TransactionId, string Message);