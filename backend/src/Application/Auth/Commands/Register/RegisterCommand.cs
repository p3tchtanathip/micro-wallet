using MediatR;

namespace Application.Auth.Commands.Register;

public record class RegisterCommand(
    string Email, 
    string Password, 
    string? FullName) : IRequest<long>;
