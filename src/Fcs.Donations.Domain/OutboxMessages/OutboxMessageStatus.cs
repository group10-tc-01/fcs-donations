namespace Fcs.Donations.Domain.OutboxMessages;

public enum OutboxMessageStatus
{
    Pending,
    Published,
    Failed
}
