using Fcs.Donations.Domain.Results;
using MediatR;

namespace Fcs.Donations.Application.Abstractions.Messaging;

public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
