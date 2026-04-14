namespace Peerly.Auth.Models.User;

public sealed record UserUpdateItem
{
    public required bool IsConfirmed { get; init; }
}
