using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.Application.UseCases.Items.GetItemById;

public interface IGetItemByIdUseCase : IQueryHandler<GetItemByIdRequest, GetItemByIdResponse>
{
}
