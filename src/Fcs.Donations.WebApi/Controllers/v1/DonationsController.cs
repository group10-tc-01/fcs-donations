using Fcs.Donations.Application.Abstractions.Authentication;
using Fcs.Donations.Application.UseCases.Donations.CreateDonation;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.Messages;
using Fcs.Donations.WebApi.Extensions;
using Fcs.Donations.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;

namespace Fcs.Donations.WebApi.Controllers.v1;

[Authorize(Roles = "Doador")]
public sealed class DonationsController : BaseApiController
{
    private readonly IDonationQueryService _donationQueryService;
    private readonly ILoggedUserService _loggedUser;

    public DonationsController(
        IMediator mediator,
        IDonationQueryService donationQueryService,
        ILoggedUserService loggedUser) : base(mediator)
    {
        _donationQueryService = donationQueryService;
        _loggedUser = loggedUser;
    }

    [HttpGet]
    [EnableQuery(MaxTop = 100)]
    [ProducesResponseType(typeof(IQueryable<DonationQueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public IActionResult Get()
    {
        var donorId = _loggedUser.GetUserId();

        if (donorId is null)
        {
            return Unauthorized(ApiResponse<string>.FromFailure(ResourceMessages.DonationUnauthenticated));
        }

        return Ok(_donationQueryService.QueryByDonor(donorId.Value));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CreateDonationResponse>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDonationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(request, cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Accepted(ApiResponse<CreateDonationResponse>.FromSuccess(result.Value));
    }
}
