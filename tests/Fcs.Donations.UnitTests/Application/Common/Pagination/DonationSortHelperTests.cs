using Fcs.Donations.Application.Common.Pagination;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.Domain.Donations;
using FluentAssertions;

namespace Fcs.Donations.UnitTests.Application.Common.Pagination;

public sealed class DonationSortHelperTests
{
    private static readonly List<DonationQueryResponse> Donations =
    [
        new() { Id = Guid.NewGuid(), Amount = 100, Status = DonationStatus.Processed, CreatedAt = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
        new() { Id = Guid.NewGuid(), Amount = 50, Status = DonationStatus.Pending, CreatedAt = new(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc) },
        new() { Id = Guid.NewGuid(), Amount = 200, Status = DonationStatus.Failed, CreatedAt = new(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc) }
    ];

    [Fact]
    public void ApplyPagination_SortByAmountAsc_ShouldReturnOrderedByAmount()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 1, 10, "amount", false);

        result.Items.Select(x => x.Amount).Should().BeInAscendingOrder();
    }

    [Fact]
    public void ApplyPagination_SortByAmountDesc_ShouldReturnOrderedByAmountDescending()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 1, 10, "amount", true);

        result.Items.Select(x => x.Amount).Should().BeInDescendingOrder();
    }

    [Fact]
    public void ApplyPagination_SortByStatusAsc_ShouldReturnOrderedByStatus()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 1, 10, "status", false);

        result.Items.Select(x => x.Status).Should().BeInAscendingOrder();
    }

    [Fact]
    public void ApplyPagination_SortByStatusDesc_ShouldReturnOrderedByStatusDescending()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 1, 10, "status", true);

        result.Items.Select(x => x.Status).Should().BeInDescendingOrder();
    }

    [Fact]
    public void ApplyPagination_SortByCreatedAtAsc_ShouldReturnOrderedByCreatedAt()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 1, 10, "createdat", false);

        result.Items.Select(x => x.CreatedAt).Should().BeInAscendingOrder();
    }

    [Fact]
    public void ApplyPagination_SortByCreatedAtDesc_ShouldReturnOrderedByCreatedAtDescending()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 1, 10, "createdat", true);

        result.Items.Select(x => x.CreatedAt).Should().BeInDescendingOrder();
    }

    [Fact]
    public void ApplyPagination_DefaultSortBy_ShouldFallbackToCreatedAtAsc()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 1, 10, null, false);

        result.Items.Select(x => x.CreatedAt).Should().BeInAscendingOrder();
    }

    [Fact]
    public void ApplyPagination_DefaultSortBy_ShouldFallbackToCreatedAtDesc()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 1, 10, null, true);

        result.Items.Select(x => x.CreatedAt).Should().BeInDescendingOrder();
    }

    [Fact]
    public void ApplyPagination_PageZero_ShouldNormalizeToPageOne()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 0, 10, null, false);

        result.Page.Should().Be(1);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public void ApplyPagination_PageSizeZero_ShouldNormalizeToDefault()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 1, 0, null, false);

        result.PageSize.Should().Be(10);
    }

    [Fact]
    public void ApplyPagination_PageSizeAboveMax_ShouldNormalizeToDefault()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 1, 101, null, false);

        result.PageSize.Should().Be(10);
    }

    [Fact]
    public void ApplyPagination_SecondPage_ShouldSkipFirstPageItems()
    {
        var result = DonationSortHelper.ApplyPagination(Donations, 2, 1, null, false);

        result.Items.Should().HaveCount(1);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(1);
        result.TotalCount.Should().Be(3);
    }
}
