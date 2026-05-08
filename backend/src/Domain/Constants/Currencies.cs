namespace Domain.Constants;

public static class Currencies
{
    public const string THB = "THB";
    public const string USD = "USD";

    public static readonly IReadOnlyCollection<string> All = new[] { THB, USD };
}