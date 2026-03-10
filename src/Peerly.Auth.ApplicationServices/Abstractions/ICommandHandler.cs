using System.Threading;
using System.Threading.Tasks;
using Peerly.Auth.ApplicationServices.Models.Common;

namespace Peerly.Auth.ApplicationServices.Abstractions;

public interface ICommandHandler<in TCommand, TSuccess>
    where TCommand : ICommand<TSuccess>
{
    Task<CommandResponse<TSuccess>> ExecuteAsync(TCommand command, CancellationToken cancellationToken);
}
