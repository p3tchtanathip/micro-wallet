using Application.Common.Interfaces;
using Google.Apis.Auth;

namespace Infrastructure.Security;

public class GoogleAuthService : IGoogleAuthService
{
    public async Task<GoogleUserPayload?> VerifyTokenAsync(string idToken)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken);
            return new GoogleUserPayload(payload.Email, payload.Name, payload.Subject);
        }
        catch
        {
            return null;
        }
    }
}