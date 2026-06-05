using Fcs.Donations.Application.Abstractions.Messaging;

namespace Fcs.Donations.Application.UseCases.Items.GetItemById;

public sealed record GetItemByIdRequest(Guid Id) : IQuery<GetItemByIdResponse>;
