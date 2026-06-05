using fcs.Donations.Application.UseCases.Donations.CreateDonation;
using fcs.Donations.WebApi.Extensions;
using fcs.Donations.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace fcs.Donations.WebApi.Controllers.v1;

[Authorize(Roles = "Doador")]
public sealed class DonationsController : BaseApiController
{
    public DonationsController(IMediator mediator) : base(mediator)
    {
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
