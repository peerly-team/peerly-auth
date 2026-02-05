using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Generators.AuthTokens;
using Peerly.Auth.ApplicationServices.Providers.AuthTokens.Abstractions;
using Peerly.Auth.Tools;

namespace Peerly.Auth.ApplicationServices.Features.Auth.GetJwks;

internal sealed class GetJwksHandler : IQueryHandler<GetJwksQuery, GetJwksQueryResponse>
{
    private readonly ISigningKeyProvider _signingKeyProvider;
    private readonly IJsonSerializationService _jsonSerializationService;

    public GetJwksHandler(ISigningKeyProvider signingKeyProvider, IJsonSerializationService jsonSerializationService)
    {
        _signingKeyProvider = signingKeyProvider;
        _jsonSerializationService = jsonSerializationService;
    }

    public async Task<GetJwksQueryResponse> ExecuteAsync(GetJwksQuery query, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken); // todo: добавить запись в БД, что кто-то запрашивал токены и что мы ему вернули

        var publicKeys = _signingKeyProvider.GetRsaPublicKeys();
        var jwks = JwkGenerator.MassCreate(publicKeys);

        return new GetJwksQueryResponse
        {
            Jwks = jwks.ToArrayBy(jwk => _jsonSerializationService.Serialize(jwk))
        };
    }
}
