using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.ApplicationServices.Models.Common;

namespace Peerly.Auth.ApplicationServices.Abstractions;

internal interface ICommandValidator<in TCommand, TCommandResponse>
    where TCommand : ICommand<TCommandResponse>
{
    Task<CommandValidationResult> ValidateAsync(TCommand command, CancellationToken cancellationToken);
}
