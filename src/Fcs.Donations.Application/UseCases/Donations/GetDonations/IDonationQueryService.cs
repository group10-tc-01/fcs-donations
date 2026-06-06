namespace Fcs.Donations.Application.UseCases.Donations.GetDonations;

public interface IDonationQueryService
{
    IQueryable<DonationQueryResponse> QueryByDonor(Guid donorId);
}
