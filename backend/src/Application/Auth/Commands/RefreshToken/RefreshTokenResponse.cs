namespace Application.Auth.Commands.RefreshToken;

public record RefreshTokenResponse(
    string AccessToken, 
    string RefreshToken
);
