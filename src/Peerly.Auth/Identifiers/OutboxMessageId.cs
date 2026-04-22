namespace Peerly.Auth.Identifiers;

public struct OutboxMessageId
{
    private readonly long _value;

    public OutboxMessageId(long value) => _value = value;

    public int CompareTo(OutboxMessageId other) => _value.CompareTo(other._value);

    public bool Equals(OutboxMessageId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is OutboxMessageId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator OutboxMessageId(long value) => new(value);

    public static explicit operator long(OutboxMessageId value) => value._value;

    public static bool operator ==(OutboxMessageId left, OutboxMessageId right) => left.Equals(right);

    public static bool operator !=(OutboxMessageId left, OutboxMessageId right) => !left.Equals(right);

    public static bool operator <(OutboxMessageId left, OutboxMessageId right) => left._value < right._value;

    public static bool operator >(OutboxMessageId left, OutboxMessageId right) => left._value > right._value;

    public static bool operator <=(OutboxMessageId left, OutboxMessageId right) => left._value <= right._value;

    public static bool operator >=(OutboxMessageId left, OutboxMessageId right) => left._value >= right._value;

    public static OutboxMessageId Empty => default;
}
