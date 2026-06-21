using System.Security.Claims;
using Fcs.Donations.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Fcs.Donations.Infrastructure.Auth.Authentication;

public sealed class LoggedUserService : ILoggedUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LoggedUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? GetUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }
}
