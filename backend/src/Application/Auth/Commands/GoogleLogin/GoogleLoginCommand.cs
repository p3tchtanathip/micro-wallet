using Application.Auth.Commands.Login;
using MediatR;

namespace Application.Auth.Commands.GoogleLogin;

public record GoogleLoginCommand(string IdToken) : IRequest<LoginResponse>;