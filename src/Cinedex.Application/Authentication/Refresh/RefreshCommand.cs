namespace Cinedex.Application.Authentication.Refresh;

public sealed class RefreshCommand
{
    public required string RefreshToken { get; set; }
}