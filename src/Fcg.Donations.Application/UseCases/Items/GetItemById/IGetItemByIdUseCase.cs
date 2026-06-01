using Fcg.Donations.Application.Abstractions.Messaging;

namespace Fcg.Donations.Application.UseCases.Items.GetItemById;

public interface IGetItemByIdUseCase : IQueryHandler<GetItemByIdRequest, GetItemByIdResponse>
{
}
