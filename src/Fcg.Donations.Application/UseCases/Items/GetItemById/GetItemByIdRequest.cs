using Fcg.Donations.Application.Abstractions.Messaging;

namespace Fcg.Donations.Application.UseCases.Items.GetItemById;

public sealed record GetItemByIdRequest(Guid Id) : IQuery<GetItemByIdResponse>;
