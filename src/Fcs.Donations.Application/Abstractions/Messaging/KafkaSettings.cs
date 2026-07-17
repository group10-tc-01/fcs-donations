namespace Fcs.Donations.Application.Abstractions.Messaging;

public sealed class KafkaSettings
{
    public const string SectionName = "KafkaSettings";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public KafkaTopicsSettings Topics { get; set; } = new();
}

public sealed class KafkaTopicsSettings
{
    public string AuditLog { get; set; } = "audit-log-requested";
    public string EmailNotification { get; set; } = "email-notification-requested";
}
