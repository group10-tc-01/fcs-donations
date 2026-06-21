using Fcs.Donations.Domain;
using MediatR;

namespace Fcs.Donations.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
