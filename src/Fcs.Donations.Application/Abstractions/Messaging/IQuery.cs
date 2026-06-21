using Fcs.Donations.Domain.Results;
using MediatR;

namespace Fcs.Donations.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
