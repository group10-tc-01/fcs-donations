namespace fcs.Donations.Application.Abstractions.Authentication;

public interface ILoggedUserService
{
    Guid? GetUserId();
}
