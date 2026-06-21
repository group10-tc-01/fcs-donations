namespace Fcs.Donations.Domain.Donations;

public interface IDonationRepository
{
    IQueryable<Donation> Query();
    Task AddAsync(Donation donation, CancellationToken cancellationToken = default);
    Task<Donation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Donation donation, CancellationToken cancellationToken = default);
}
