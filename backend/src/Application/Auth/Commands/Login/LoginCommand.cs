using MediatR;

namespace Application.Auth.Commands.Login;

public record class LoginCommand(
    string Email, 
    string Password) : IRequest<LoginResponse>;
