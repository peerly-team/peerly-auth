using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.ApplicationServices.Abstractions;
using Peerly.Auth.ApplicationServices.Generators.AuthTokens;
using Peerly.Auth.ApplicationServices.Providers.AuthTokens.Abstractions;
using Peerly.Auth.Tools;

namespace Peerly.Auth.ApplicationServices.Features.V1.Auth.GetJwks;

internal sealed class GetJwksHandler : IQueryHandler<GetJwksQuery, GetJwksQueryResponse>
{
    private readonly ISigningKeyProvider _signingKeyProvider;
    private readonly IJsonSerializationService _jsonSerializationService;

    public GetJwksHandler(ISigningKeyProvider signingKeyProvider, IJsonSerializationService jsonSerializationService)
    {
        _signingKeyProvider = signingKeyProvider;
        _jsonSerializationService = jsonSerializationService;
    }

    public Task<GetJwksQueryResponse> ExecuteAsync(GetJwksQuery query, CancellationToken cancellationToken)
    {
        var publicKeys = _signingKeyProvider.GetRsaPublicKeys();
        var jwks = JwkGenerator.MassCreate(publicKeys);

        return Task.FromResult(new GetJwksQueryResponse
        {
            Jwks = jwks.ToArrayBy(jwk => _jsonSerializationService.Serialize(jwk))
        });
    }
}
