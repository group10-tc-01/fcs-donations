namespace Fcg.Donations.Domain.OutboxMessages;

public enum OutboxMessageStatus
{
    Pending,
    Published,
    Failed
}
