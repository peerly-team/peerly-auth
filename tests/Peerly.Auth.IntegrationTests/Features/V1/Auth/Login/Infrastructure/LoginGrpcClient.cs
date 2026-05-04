using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Auth.V1;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Login.Infrastructure;

public sealed class LoginGrpcClient
{
    private static readonly Method<V1LoginRequest, V1LoginResponse> s_loginMethod = new(
        MethodType.Unary,
        "peerly.auth.v1.AuthService",
        "V1Login",
        CreateMarshaller(V1LoginRequest.Parser),
        CreateMarshaller(V1LoginResponse.Parser));

    private readonly GrpcChannel _channel;

    public LoginGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1LoginResponse> V1LoginAsync(
        V1LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_loginMethod,
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
