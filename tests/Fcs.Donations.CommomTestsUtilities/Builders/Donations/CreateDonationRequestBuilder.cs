using System.Diagnostics.CodeAnalysis;
using Bogus;
using Fcs.Donations.Application.UseCases.Donations.CreateDonation;

namespace Fcs.Donations.CommomTestsUtilities.Builders.Donations;

[ExcludeFromCodeCoverage]
public sealed class CreateDonationRequestBuilder
{
    private readonly Faker<CreateDonationRequest> _faker = new Faker<CreateDonationRequest>()
        .CustomInstantiator(f => new CreateDonationRequest(
            f.Random.Guid(),
            decimal.Parse(f.Commerce.Price(10, 500))));

    public CreateDonationRequest Build() => _faker.Generate();
}
