namespace Peerly.Auth.Identifiers;

public struct UserId
{
    private readonly long _value;

    public UserId(long value) => _value = value;

    public int CompareTo(UserId other) => _value.CompareTo(other._value);

    public bool Equals(UserId other) => _value == other._value;

    public override bool Equals(object? obj) => obj is UserId id && Equals(id);

    public override int GetHashCode() => _value.GetHashCode();

    public override string ToString() => _value.ToString();

    public static explicit operator UserId(long value) => new(value);

    public static explicit operator long(UserId value) => value._value;

    public static bool operator ==(UserId left, UserId right) => left.Equals(right);

    public static bool operator !=(UserId left, UserId right) => !left.Equals(right);

    public static bool operator <(UserId left, UserId right) => left._value < right._value;

    public static bool operator >(UserId left, UserId right) => left._value > right._value;

    public static bool operator <=(UserId left, UserId right) => left._value <= right._value;

    public static bool operator >=(UserId left, UserId right) => left._value >= right._value;

    public static UserId Empty => default;
}
