namespace Cinedex.Application.Authentication.Login;

public sealed class LoginCommand
{
    public required string Email { get; set; }
    
    public required string Password { get; set; }
}