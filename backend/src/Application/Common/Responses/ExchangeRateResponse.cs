namespace Application.Common.Responses;

public class ExchangeRateResponse
{
    public Dictionary<string, decimal> Rates { get; set; } = [];
}