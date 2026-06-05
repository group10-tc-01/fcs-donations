using FluentValidation;

namespace fcs.Donations.Application.UseCases.Donations.CreateDonation;

public sealed class CreateDonationRequestValidator : AbstractValidator<CreateDonationRequest>
{
    public CreateDonationRequestValidator()
    {
        RuleFor(x => x.CampaignId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);
    }
}
