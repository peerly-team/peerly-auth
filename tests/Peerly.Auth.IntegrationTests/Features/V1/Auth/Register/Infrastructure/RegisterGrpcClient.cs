using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Auth.V1;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.Register.Infrastructure;

public sealed class RegisterGrpcClient
{
    private static readonly Method<V1RegisterRequest, V1RegisterResponse> s_registerMethod = new(
        MethodType.Unary,
        "peerly.auth.v1.AuthService",
        "V1Register",
        CreateMarshaller(V1RegisterRequest.Parser),
        CreateMarshaller(V1RegisterResponse.Parser));

    private readonly GrpcChannel _channel;

    public RegisterGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1RegisterResponse> V1RegisterAsync(
        V1RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_registerMethod,
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
