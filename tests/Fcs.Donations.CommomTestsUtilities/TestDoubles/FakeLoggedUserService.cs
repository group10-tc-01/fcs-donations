using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.Abstractions.Authentication;

namespace Fcs.Donations.CommomTestsUtilities.TestDoubles;

[ExcludeFromCodeCoverage]
public sealed class FakeLoggedUserService : ILoggedUserService
{
    public Guid? UserId { get; set; } = Guid.NewGuid();

    public Guid? GetUserId() => UserId;
}
