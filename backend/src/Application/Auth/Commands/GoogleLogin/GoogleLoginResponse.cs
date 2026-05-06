namespace Application.Auth.Commands.GoogleLogin;

public record class GoogleLoginResponse(
    string Email, 
    string Name, 
    string GoogleId);