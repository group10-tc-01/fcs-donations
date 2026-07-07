using Fcs.Donations.Application.Audit;
using Fcs.Donations.CommomTestsUtilities.TestDoubles;
using Fcs.Donations.Domain.OutboxMessages;
using Fcs.Donations.Infrastructure.Kafka.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fcs.Donations.UnitTests.Infrastructure.Kafka;

public sealed class OutboxMessageProcessorTests
{
    [Fact]
    public async Task Given_PendingMessage_When_ProcessPendingAsync_Then_ShouldPublishMarkPublishedPersistAndAudit()
    {
        var repository = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakeOutboxMessagePublisher();
        var auditPublisher = new FakeAuditPublisher();
        var message = new OutboxMessage(Guid.NewGuid(), Guid.NewGuid(), "DonationReceivedEvent", "{}");
        await repository.AddAsync(message);
        var sut = CreateSut(repository, publisher, unitOfWork, auditPublisher);

        await sut.ProcessPendingAsync(10, CancellationToken.None);

        publisher.Payloads.Should().ContainSingle("{}");
        message.Status.Should().Be(OutboxMessageStatus.Published);
        message.PublishedAt.Should().NotBeNull();
        unitOfWork.SaveChangesCalls.Should().Be(1);
        auditPublisher.Events.Should().ContainSingle(e => e.Action == AuditActions.DonationEventPublished);
    }

    [Fact]
    public async Task Given_PublisherFailure_When_ProcessPendingAsync_Then_ShouldMarkFailedAndPersist()
    {
        var repository = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var publisher = new FakeOutboxMessagePublisher { Error = new InvalidOperationException("Kafka unavailable.") };
        var auditPublisher = new FakeAuditPublisher();
        var message = new OutboxMessage(Guid.NewGuid(), Guid.NewGuid(), "DonationReceivedEvent", "{}");
        await repository.AddAsync(message);
        var sut = CreateSut(repository, publisher, unitOfWork, auditPublisher);

        await sut.ProcessPendingAsync(10, CancellationToken.None);

        message.Status.Should().Be(OutboxMessageStatus.Failed);
        message.LastError.Should().Be("Kafka unavailable.");
        message.RetryCount.Should().Be(1);
        unitOfWork.SaveChangesCalls.Should().Be(1);
        auditPublisher.Events.Should().BeEmpty();
    }

    private static OutboxMessageProcessor CreateSut(
        InMemoryOutboxMessageRepository repository,
        FakeOutboxMessagePublisher publisher,
        FakeUnitOfWork unitOfWork,
        FakeAuditPublisher auditPublisher)
    {
        return new OutboxMessageProcessor(
            repository,
            publisher,
            unitOfWork,
            auditPublisher,
            NullLogger<OutboxMessageProcessor>.Instance);
    }

    private sealed class FakeOutboxMessagePublisher : IOutboxMessagePublisher
    {
        public List<string> Payloads { get; } = new();
        public Exception? Error { get; init; }

        public Task PublishRawAsync(string payload, CancellationToken cancellationToken = default)
        {
            if (Error is not null)
            {
                throw Error;
            }

            Payloads.Add(payload);
            return Task.CompletedTask;
        }
    }
}
