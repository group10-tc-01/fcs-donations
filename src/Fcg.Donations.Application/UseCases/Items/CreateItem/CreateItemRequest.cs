using Fcg.Donations.Application.Abstractions.Messaging;

namespace Fcg.Donations.Application.UseCases.Items.CreateItem;

public sealed record CreateItemRequest(string Name, decimal Price) : ICommand<CreateItemResponse>;
