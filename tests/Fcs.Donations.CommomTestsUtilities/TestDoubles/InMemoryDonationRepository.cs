using fcs.Donations.Domain.Donations;

namespace fcs.Donations.CommomTestsUtilities.TestDoubles;

public sealed class InMemoryDonationRepository : IDonationRepository
{
    private readonly Dictionary<Guid, Donation> _donations = new();

    public Task AddAsync(Donation donation, CancellationToken cancellationToken = default)
    {
        _donations[donation.Id] = donation;
        return Task.CompletedTask;
    }

    public Task<Donation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _donations.TryGetValue(id, out var donation);
        return Task.FromResult(donation);
    }

    public Task UpdateAsync(Donation donation, CancellationToken cancellationToken = default)
    {
        _donations[donation.Id] = donation;
        return Task.CompletedTask;
    }
}
