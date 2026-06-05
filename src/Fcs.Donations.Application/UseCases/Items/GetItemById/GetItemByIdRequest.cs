using fcs.Donations.Application.Abstractions.Messaging;

namespace fcs.Donations.Application.UseCases.Items.GetItemById;

public sealed record GetItemByIdRequest(Guid Id) : IQuery<GetItemByIdResponse>;
