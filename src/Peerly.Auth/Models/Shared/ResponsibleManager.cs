namespace Peerly.Auth.Models.Shared;

public sealed record ResponsibleManager
{
    public required string Login { get; init; }
    public required string Name { get; init; }
}
