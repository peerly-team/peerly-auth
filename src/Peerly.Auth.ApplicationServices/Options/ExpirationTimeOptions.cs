namespace Peerly.Auth.ApplicationServices.Options;

internal sealed class ExpirationTimeOptions
{
    public const string SectionName = "ExpirationTimeOptions";

    public short EmailVerificationTokenValidityPeriodMinutes { get; init; }
    public short AccessTokenPeriodMinutes { get; init; }
    public short RefreshTokenPeriodDays { get; init; }
}
