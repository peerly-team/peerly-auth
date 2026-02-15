using System;

namespace Peerly.Auth.Models.Sessions;

public sealed record SessionUpdateItem
{
    public required long Id { get; init; }
    public required string RefreshTokenHash { get; init; }
    public required DateTimeOffset CancellationTime { get; init; }
}
