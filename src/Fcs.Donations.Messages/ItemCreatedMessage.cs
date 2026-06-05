namespace fcs.Donations.Messages;

public sealed record ItemCreatedMessage(Guid Id, string Name, decimal Price, DateTime CreatedAt);
