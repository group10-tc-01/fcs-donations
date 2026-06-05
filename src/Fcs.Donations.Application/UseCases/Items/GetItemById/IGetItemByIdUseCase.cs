using fcs.Donations.Application.Abstractions.Messaging;

namespace fcs.Donations.Application.UseCases.Items.GetItemById;

public interface IGetItemByIdUseCase : IQueryHandler<GetItemByIdRequest, GetItemByIdResponse>
{
}
