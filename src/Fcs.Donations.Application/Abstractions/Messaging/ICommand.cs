using Fcs.Donations.Domain.Results;
using MediatR;

namespace Fcs.Donations.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
