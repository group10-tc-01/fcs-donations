using MediatR;
using Fcs.Donations.Domain;

namespace Fcs.Donations.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
