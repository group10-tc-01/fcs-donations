using Fcg.Donations.Application.Abstractions.Authentication;

namespace Fcg.Donations.CommomTestsUtilities.TestDoubles;

public sealed class FakeLoggedUserService : ILoggedUserService
{
    public Guid? UserId { get; set; } = Guid.NewGuid();

    public Guid? GetUserId() => UserId;
}
