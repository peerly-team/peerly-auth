namespace Peerly.Auth.Models.Auth;

public sealed record SigningKey
{
    public string PrivateKey { get; set; } = null!;
    public string Kid { get; set; } = null!;
    public bool IsActive { get; set; }
}
