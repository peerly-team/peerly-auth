using System.Threading;
using System.Threading.Tasks;
using MimeKit;

namespace Peerly.Auth.ApplicationServices.BackgroundServices.EmailVerification.Abstractions;

internal interface IEmailSender
{
    Task SendAsync(MimeMessage message, CancellationToken cancellationToken);
}
