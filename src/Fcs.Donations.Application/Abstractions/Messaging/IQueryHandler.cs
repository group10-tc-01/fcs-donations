using Fcs.Donations.Domain.Results;
using MediatR;

namespace Fcs.Donations.Application.Abstractions.Messaging;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
