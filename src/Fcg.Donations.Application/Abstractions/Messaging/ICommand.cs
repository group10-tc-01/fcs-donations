using MediatR;
using Fcg.Donations.Domain;

namespace Fcg.Donations.Application.Abstractions.Messaging;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
