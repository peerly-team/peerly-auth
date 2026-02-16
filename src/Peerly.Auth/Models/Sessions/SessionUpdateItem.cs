using System;

namespace Peerly.Auth.Models.Sessions;

public sealed record SessionUpdateItem
{
    public required DateTimeOffset CancellationTime { get; init; }
    public required DateTimeOffset UpdateTime { get; init; }
}
