namespace Fcg.Donations.Application.Abstractions.Authentication;

public interface ILoggedUserService
{
    Guid? GetUserId();
}
