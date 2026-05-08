using Application.Common.Interfaces;

namespace Infrastructure.Services;

public class PaymentGatewayService : IPaymentGatewayService
{
    public async Task<GatewayResult> DepositAsync(decimal amount)
    {
        await Task.Delay(1000);
        return new GatewayResult(
            Success: true, 
            TransactionId: Guid.NewGuid().ToString(), 
            Message: "Deposit success");
    }

    public async Task<GatewayResult> WithdrawAsync(decimal amount)
    {
        await Task.Delay(1000);
         return new GatewayResult(
            Success: true, 
            TransactionId: Guid.NewGuid().ToString(), 
            Message: "Withdraw success");
    }
}
