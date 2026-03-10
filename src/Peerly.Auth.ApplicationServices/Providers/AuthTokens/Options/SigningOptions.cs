using System.Collections.Generic;
using Peerly.Auth.Models.Auth;

namespace Peerly.Auth.ApplicationServices.Providers.AuthTokens.Options;

internal sealed class SigningOptions
{
    public const string SectionName = "SigningOptions";

    public IReadOnlyCollection<SigningKey> SigningKeys { get; init; } = [];
}
