using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Auth.V1;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.GetJwks.Infrastructure;

public sealed class GetJwksGrpcClient
{
    private static readonly Method<V1GetJwksRequest, V1GetJwksResponse> s_getJwksMethod = new(
        MethodType.Unary,
        "peerly.auth.v1.AuthService",
        "V1GetJwks",
        CreateMarshaller(V1GetJwksRequest.Parser),
        CreateMarshaller(V1GetJwksResponse.Parser));

    private readonly GrpcChannel _channel;

    public GetJwksGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1GetJwksResponse> V1GetJwksAsync(
        V1GetJwksRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_getJwksMethod,
                host: null,
                options: new CallOptions(cancellationToken: cancellationToken),
                request);

        return await call.ResponseAsync;
    }

    private static Marshaller<TMessage> CreateMarshaller<TMessage>(MessageParser<TMessage> parser)
        where TMessage : class, IMessage<TMessage>
    {
        return Marshallers.Create(
            message => message.ToByteArray(),
            parser.ParseFrom);
    }
}
