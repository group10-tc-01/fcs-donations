using Fcs.Donations.Application.Audit;
using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.CommomTestsUtilities.TestDoubles;
using Fcs.Donations.Domain.OutboxMessages;
using Fcs.Donations.Infrastructure.Kafka.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Fcs.Donations.UnitTests.Infrastructure.Kafka;

public sealed class OutboxMessageProcessorTests
{
    [Fact]
    public async Task Given_PendingMessage_When_ProcessPendingAsync_Then_ShouldPublishMarkPublishedPersistAndAudit()
    {
        var repository = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakeMessagePublisher();
        var message = new OutboxMessage(Guid.NewGuid(), Guid.NewGuid(), "DonationReceivedEvent", "{}");
        await repository.AddAsync(message);
        var sut = CreateSut(repository, publisher, unitOfWork);

        await sut.ProcessPendingAsync(10, CancellationToken.None);

        publisher.PublishedMessages.Should().Contain("{}");
        publisher.Topics.Should().Contain("donation-received");
        message.Status.Should().Be(OutboxMessageStatus.Published);
        message.PublishedAt.Should().NotBeNull();
        unitOfWork.SaveChangesCalls.Should().Be(1);
        publisher.PublishedMessages.OfType<AuditLogRequestedEvent>().Should().ContainSingle(e => e.Action == AuditActions.DonationEventPublished);
        publisher.Topics.Should().Contain("audit-log-requested");
    }

    [Fact]
    public async Task Given_PublisherFailure_When_ProcessPendingAsync_Then_ShouldMarkFailedAndPersist()
    {
        var repository = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakeMessagePublisher
        {
            ExceptionToThrow = new InvalidOperationException("Kafka unavailable."),
            ExceptionTopicName = "donation-received"
        };
        var message = new OutboxMessage(Guid.NewGuid(), Guid.NewGuid(), "DonationReceivedEvent", "{}");
        await repository.AddAsync(message);
        var sut = CreateSut(repository, publisher, unitOfWork);

        await sut.ProcessPendingAsync(10, CancellationToken.None);

        message.Status.Should().Be(OutboxMessageStatus.Failed);
        message.LastError.Should().Be("Kafka unavailable.");
        message.RetryCount.Should().Be(1);
        unitOfWork.SaveChangesCalls.Should().Be(1);
        publisher.PublishedMessages.Should().BeEmpty();
    }

    private static OutboxMessageProcessor CreateSut(
        InMemoryOutboxMessageRepository repository,
        FakeMessagePublisher publisher,
        FakeUnitOfWork unitOfWork)
    {
        return new OutboxMessageProcessor(
            repository,
            publisher,
            unitOfWork,
            Options.Create(new KafkaSettings()),
            NullLogger<OutboxMessageProcessor>.Instance);
    }
}
