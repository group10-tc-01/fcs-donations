using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.Infrastructure.Kafka.Settings;

[ExcludeFromCodeCoverage]
public sealed class KafkaSettings
{
    public const string SectionName = "KafkaSettings";

    public string BootstrapServers { get; set; } = "localhost:9092";
    public string TopicName { get; set; } = "donation-received";
    public string AuditTopicName { get; set; } = "audit-log-requested";
}
