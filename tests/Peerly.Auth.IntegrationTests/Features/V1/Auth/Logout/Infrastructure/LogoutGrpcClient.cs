using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Auth.V1;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Logout.Infrastructure;

public sealed class LogoutGrpcClient
{
    private static readonly Method<V1LogoutRequest, V1LogoutResponse> s_logoutMethod = new(
        MethodType.Unary,
        "peerly.auth.v1.AuthService",
        "V1Logout",
        CreateMarshaller(V1LogoutRequest.Parser),
        CreateMarshaller(V1LogoutResponse.Parser));

    private readonly GrpcChannel _channel;

    public LogoutGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1LogoutResponse> V1LogoutAsync(
        V1LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_logoutMethod,
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
