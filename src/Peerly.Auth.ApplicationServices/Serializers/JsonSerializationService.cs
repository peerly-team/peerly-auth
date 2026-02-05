using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using Peerly.Auth.ApplicationServices.Abstractions;

namespace Peerly.Auth.ApplicationServices.Serializers;

internal sealed class JsonSerializationService : IJsonSerializationService
{
    private readonly JsonSerializerOptions _serializerOptions;

    public JsonSerializationService(IOptions<JsonOptions> jsonOptions)
    {
        _serializerOptions = jsonOptions.Value.SerializerOptions;
    }

    public TValue Deserialize<TValue>([StringSyntax(StringSyntaxAttribute.Json)] string json)
    {
        return JsonSerializer.Deserialize<TValue>(json, _serializerOptions)
               ?? throw new SerializationException(
                   $"""
                    Deserialize error.
                    Type: {typeof(TValue).Name}.
                    Json value: {json}.
                    """);
    }

    public string Serialize<TValue>(TValue value)
    {
        return JsonSerializer.Serialize(value, _serializerOptions);
    }
}
