using Fcs.Donations.Application.UseCases.Donations.GetDonations;

namespace Fcs.Donations.Application.Common.Pagination;

public static class DonationSortHelper
{
    public static PagedResponse<DonationQueryResponse> ApplyPagination(
        List<DonationQueryResponse> donations,
        int page,
        int pageSize,
        string? sortBy,
        bool sortDescending)
    {
        var normalizedPage = PagedResponse<DonationQueryResponse>.NormalizePage(page);
        var normalizedPageSize = PagedResponse<DonationQueryResponse>.NormalizePageSize(pageSize);

        var sorted = (sortBy?.ToLowerInvariant(), sortDescending) switch
        {
            ("amount", false) => donations.OrderBy(x => x.Amount),
            ("amount", true) => donations.OrderByDescending(x => x.Amount),
            ("status", false) => donations.OrderBy(x => x.Status),
            ("status", true) => donations.OrderByDescending(x => x.Status),
            ("createdat", false) => donations.OrderBy(x => x.CreatedAt),
            ("createdat", true) => donations.OrderByDescending(x => x.CreatedAt),
            (_, false) => donations.OrderBy(x => x.CreatedAt),
            (_, true) => donations.OrderByDescending(x => x.CreatedAt)
        };

        var totalCount = sorted.Count();
        var items = sorted
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToList();

        return new PagedResponse<DonationQueryResponse>(items, normalizedPage, normalizedPageSize, totalCount);
    }
}
