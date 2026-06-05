using MediatR;
using fcs.Donations.Domain;

namespace fcs.Donations.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
