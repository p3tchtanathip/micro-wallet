namespace Application.Common.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleUserPayload?> VerifyTokenAsync(string idToken);
}

public record GoogleUserPayload(string Email, string Name, string GoogleId);