using MediatR;
using Fcs.Donations.Domain;

namespace Fcs.Donations.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
