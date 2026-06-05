namespace Fcs.Donations.Application.Abstractions.Authentication;

public interface ILoggedUserService
{
    Guid? GetUserId();
}
