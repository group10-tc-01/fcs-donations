using Fcs.Donations.Domain;
using MediatR;

namespace Fcs.Donations.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
