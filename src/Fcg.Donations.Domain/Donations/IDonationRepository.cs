namespace Fcg.Donations.Domain.Donations;

public interface IDonationRepository
{
    Task AddAsync(Donation donation, CancellationToken cancellationToken = default);
    Task<Donation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateAsync(Donation donation, CancellationToken cancellationToken = default);
}
