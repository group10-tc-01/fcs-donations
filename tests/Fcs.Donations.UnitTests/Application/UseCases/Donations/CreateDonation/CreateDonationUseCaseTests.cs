using Fcs.Donations.Application.Audit;
using Fcs.Donations.Application.Abstractions.Messaging;
using Fcs.Donations.Application.UseCases.Donations.CreateDonation;
using Fcs.Donations.CommomTestsUtilities.Builders.Donations;
using Fcs.Donations.CommomTestsUtilities.TestDoubles;
using Fcs.Donations.Domain.Results;
using Fcs.Donations.Messages;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace Fcs.Donations.UnitTests.Application.UseCases.Donations.CreateDonation;

public sealed class CreateDonationUseCaseTests
{
    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ShouldCreateDonation()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var currentUser = new FakeCurrentUser();
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, currentUser);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Amount.Should().Be(request.Amount);
        donationRepo.Query().Should().ContainSingle(donation =>
            donation.DonorId.ToString() == currentUser.KeycloakUserId);
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task Given_CampaignNotEligible_When_Handle_Then_ShouldReturnValidationError()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient { IsEligible = false };
        var currentUser = new FakeCurrentUser();
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, currentUser);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Given_CampaignServiceFailure_When_Handle_Then_ShouldPropagateError()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient
        {
            Error = Error.ServiceUnavailable("Campaign.ServiceUnavailable", "Campaign service unavailable.")
        };
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(
            donationRepo,
            outboxRepo,
            unitOfWork,
            campaignClient,
            new FakeCurrentUser());

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.ServiceUnavailable);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task Given_UnauthenticatedCurrentUser_When_Handle_Then_ShouldReturnFailureError()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var currentUser = new FakeCurrentUser
        {
            IsAuthenticated = false,
            KeycloakUserId = null
        };
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, currentUser);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Failure);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task Given_InvalidCurrentUserId_When_Handle_Then_ShouldReturnFailureError()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var currentUser = new FakeCurrentUser { KeycloakUserId = "invalid-guid" };
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, currentUser);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Failure);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task Given_InvalidAmount_When_Handle_Then_ShouldReturnValidationError()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var currentUser = new FakeCurrentUser();
        var request = new CreateDonationRequest(Guid.NewGuid(), 0);
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, currentUser);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ShouldPersistOutboxMessage()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var currentUser = new FakeCurrentUser();
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, currentUser);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        outboxRepo.Query().Should().ContainSingle(m => m.AggregateId == result.Value.Id);
    }

    [Fact]
    public async Task Given_CampaignServiceFailure_When_Handle_Then_ShouldNotPersistDonation()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient
        {
            Error = Error.ServiceUnavailable("Campaign.ServiceUnavailable", "Campaign service unavailable.")
        };
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, new FakeCurrentUser());

        await sut.Handle(request, CancellationToken.None);

        donationRepo.Query().Should().BeEmpty();
        outboxRepo.Query().Should().BeEmpty();
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ShouldReturnCorrectCampaignId()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var currentUser = new FakeCurrentUser();
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, currentUser);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.CampaignId.Should().Be(request.CampaignId);
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ShouldPublishDonationRequestedAndEventQueuedAudits()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var currentUser = new FakeCurrentUser();
        var messagePublisher = new FakeMessagePublisher();
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, currentUser, messagePublisher);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        messagePublisher.PublishedMessages.OfType<AuditLogRequestedEvent>().Should().Contain(e =>
            e.Action == AuditActions.DonationRequested &&
            e.EntityName == "Donation" &&
            e.EntityId == result.Value.Id.ToString());
        messagePublisher.PublishedMessages.OfType<AuditLogRequestedEvent>().Should().Contain(e =>
            e.Action == AuditActions.DonationEventQueued &&
            e.EntityName == "OutboxMessage");
    }

    [Fact]
    public async Task Given_ValidRequest_When_Handle_Then_ShouldPublishDonationCreatedEmailNotification()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var currentUser = new FakeCurrentUser { Email = "doador@teste.local" };
        var messagePublisher = new FakeMessagePublisher();
        var request = new CreateDonationRequestBuilder().Build();
        var kafkaSettings = Options.Create(new KafkaSettings
        {
            Topics = new KafkaTopicsSettings { EmailNotification = "email-notification-requested-test" }
        });
        var sut = new CreateDonationUseCase(
            donationRepo,
            outboxRepo,
            unitOfWork,
            campaignClient,
            currentUser,
            messagePublisher: messagePublisher,
            kafkaSettings: kafkaSettings);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var emailNotification = messagePublisher.PublishedMessages.OfType<EmailNotificationRequestedEvent>().Should().ContainSingle().Subject;
        messagePublisher.Topics.Should().Contain("email-notification-requested-test");
        emailNotification.Type.Should().Be(EmailNotificationRequestedEvent.DonationCreated);
        emailNotification.RecipientEmail.Should().Be(currentUser.Email);
        emailNotification.DonationId.Should().Be(result.Value.Id);
        emailNotification.Amount.Should().Be(request.Amount);
        emailNotification.OccurredAt.Should().Be(result.Value.CreatedAt);
    }

    [Fact]
    public async Task Given_EmailNotificationPublisherFailure_When_Handle_Then_ShouldPersistDonationAndReturnSuccess()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var messagePublisher = new FakeMessagePublisher
        {
            ExceptionToThrow = new InvalidOperationException("Email notification broker unavailable."),
            ExceptionTopicName = "email-notification-requested"
        };
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(
            donationRepo,
            outboxRepo,
            unitOfWork,
            campaignClient,
            new FakeCurrentUser(),
            messagePublisher: messagePublisher);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        donationRepo.Query().Should().ContainSingle(donation => donation.Id == result.Value.Id);
        outboxRepo.Query().Should().ContainSingle(message => message.AggregateId == result.Value.Id);
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task Given_EmailNotificationPublisherIsCancelled_When_Handle_Then_ShouldPropagateCancellation()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var messagePublisher = new FakeMessagePublisher
        {
            ExceptionToThrow = new OperationCanceledException(cancellationTokenSource.Token),
            ExceptionTopicName = "email-notification-requested"
        };
        var sut = new CreateDonationUseCase(
            donationRepo,
            outboxRepo,
            unitOfWork,
            campaignClient,
            new FakeCurrentUser(),
            messagePublisher: messagePublisher);

        var act = () => sut.Handle(new CreateDonationRequestBuilder().Build(), cancellationTokenSource.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
        donationRepo.Query().Should().ContainSingle();
        outboxRepo.Query().Should().ContainSingle();
        unitOfWork.SaveChangesCalls.Should().Be(1);
    }

    [Fact]
    public async Task Given_AuthenticatedUserWithoutEmail_When_Handle_Then_ShouldReturnValidationError()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient();
        var currentUser = new FakeCurrentUser { Email = " " };
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, currentUser);

        var result = await sut.Handle(new CreateDonationRequestBuilder().Build(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("donation.email_missing");
        donationRepo.Query().Should().BeEmpty();
        outboxRepo.Query().Should().BeEmpty();
        unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task Given_CampaignNotEligible_When_Handle_Then_ShouldPublishDonationRejectedAudit()
    {
        var donationRepo = new InMemoryDonationRepository();
        var outboxRepo = new InMemoryOutboxMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var campaignClient = new FakeCampaignEligibilityClient { IsEligible = false };
        var currentUser = new FakeCurrentUser();
        var messagePublisher = new FakeMessagePublisher();
        var request = new CreateDonationRequestBuilder().Build();
        var sut = new CreateDonationUseCase(donationRepo, outboxRepo, unitOfWork, campaignClient, currentUser, messagePublisher);

        var result = await sut.Handle(request, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        var auditEvent = messagePublisher.PublishedMessages.OfType<AuditLogRequestedEvent>().Should().ContainSingle(e => e.Action == AuditActions.DonationRejected).Subject;
        auditEvent.EntityName.Should().Be("Donation");
        auditEvent.ActorType.Should().Be("Doador");
        auditEvent.Metadata.Should().Contain(pair => pair.Key == "reason");
    }
}
