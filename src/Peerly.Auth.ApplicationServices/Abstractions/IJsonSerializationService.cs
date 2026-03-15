using System.Diagnostics.CodeAnalysis;

namespace Peerly.Auth.ApplicationServices.Abstractions;

public interface IJsonSerializationService
{
    TValue Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json);
    string Serialize<TValue>(TValue value);
}
