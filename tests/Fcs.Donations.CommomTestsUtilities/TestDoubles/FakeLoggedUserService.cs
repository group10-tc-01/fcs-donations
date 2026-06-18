using Fcs.Donations.Application.Abstractions.Authentication;
using System.Diagnostics.CodeAnalysis;

namespace Fcs.Donations.CommomTestsUtilities.TestDoubles;

[ExcludeFromCodeCoverage]
public sealed class FakeLoggedUserService : ILoggedUserService
{
    public Guid? UserId { get; set; } = Guid.NewGuid();

    public Guid? GetUserId() => UserId;
}
