using MediatR;
using fcs.Donations.Domain;

namespace fcs.Donations.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
