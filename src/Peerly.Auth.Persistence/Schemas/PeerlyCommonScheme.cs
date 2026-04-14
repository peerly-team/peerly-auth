namespace Peerly.Auth.Persistence.Schemas;

internal static class PeerlyCommonScheme
{
    public static class UserTable
    {
        public const string TableName = "users";

        public const string Id = "id";
        public const string Email = "email";
        public const string PasswordHash = "password_hash";
        public const string Role = "role";
        public const string IsConfirmed = "is_confirmed";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class EmailVerificationTable
    {
        public const string TableName = "email_verifications";

        public const string UserId = "user_id";
        public const string Token = "token";
        public const string ExpirationTime = "expiration_time";
        public const string ProcessStatus = "process_status";
        public const string TakenTime = "taken_time";
        public const string FailCount = "fail_count";
        public const string Error = "error";
        public const string CreationTime = "creation_time";
        public const string UpdateTime = "update_time";
    }

    public static class SessionTable
    {
        public const string TableName = "sessions";

        public const string Id = "id";
        public const string UserId = "user_id";
        public const string RefreshTokenHash = "refresh_token_hash";
        public const string ExpirationTime = "expiration_time";
        public const string CreationTime = "creation_time";
        public const string CancellationTime = "cancellation_time";
        public const string UpdateTime = "update_time";
    }

    public static class OutboxMessageTable
    {
        public const string TableName = "outbox_messages";

        public const string Id = "id";
        public const string EventType = "event_type";
        public const string Topic = "topic";
        public const string Key = "key";
        public const string Payload = "payload";
        public const string CreationTime = "creation_time";
        public const string ProcessedTime = "processed_time";
        public const string FailCount = "fail_count";
        public const string Error = "error";
    }
}
