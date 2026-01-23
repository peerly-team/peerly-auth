using System.Threading;
using System.Threading.Tasks;

namespace Peerly.Auth.ApplicationServices.Abstractions;

public interface IQueryHandler<in TQuery, TQueryResponse>
    where TQuery : IQuery<TQueryResponse>
{
    Task<TQueryResponse> Execute(TQuery query, CancellationToken cancellationToken);
}
