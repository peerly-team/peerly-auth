using System;

namespace Peerly.Auth.Models.Shared;

public readonly record struct ErrorMessage
{
    private readonly string _value;

    public required string Value
    {
        get => _value;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _value = value;
        }
    }

    public static implicit operator ErrorMessage(string value) => new() { Value = value };
    public static implicit operator string(ErrorMessage errorMessage) => errorMessage._value;

    public override string ToString()
    {
        return _value;
    }
}
