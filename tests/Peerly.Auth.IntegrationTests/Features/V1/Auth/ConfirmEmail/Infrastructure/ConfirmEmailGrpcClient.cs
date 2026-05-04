using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Peerly.Auth.V1;

namespace Peerly.Auth.IntegrationTests.Features.V1.Auth.ConfirmEmail.Infrastructure;

public sealed class ConfirmEmailGrpcClient
{
    private static readonly Method<V1ConfirmEmailRequest, V1ConfirmEmailResponse> s_confirmEmailMethod = new(
        MethodType.Unary,
        "peerly.auth.v1.AuthService",
        "V1ConfirmEmail",
        CreateMarshaller(V1ConfirmEmailRequest.Parser),
        CreateMarshaller(V1ConfirmEmailResponse.Parser));

    private readonly GrpcChannel _channel;

    public ConfirmEmailGrpcClient(GrpcChannel channel)
    {
        _channel = channel;
    }

    public async Task<V1ConfirmEmailResponse> V1ConfirmEmailAsync(
        V1ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var call = _channel.CreateCallInvoker()
            .AsyncUnaryCall(
                s_confirmEmailMethod,
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
