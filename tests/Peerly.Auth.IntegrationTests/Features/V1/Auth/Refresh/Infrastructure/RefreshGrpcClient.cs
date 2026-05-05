using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Auth.V1;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Refresh.Infrastructure;

public sealed class RefreshGrpcClient
{
    private static readonly Method<V1RefreshRequest, V1RefreshResponse> s_refreshMethod = new(
        MethodType.Unary,
        "peerly.auth.v1.AuthService",
        "V1Refresh",
        CreateMarshaller(V1RefreshRequest.Parser),
        CreateMarshaller(V1RefreshResponse.Parser));

    private readonly GrpcChannel _channel;

    public RefreshGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1RefreshResponse> V1RefreshAsync(
        V1RefreshRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_refreshMethod,
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
