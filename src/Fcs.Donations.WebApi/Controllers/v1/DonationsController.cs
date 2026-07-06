using Fcs.Donations.Application.Common.Pagination;
using Fcs.Donations.Application.UseCases.Donations.CreateDonation;
using Fcs.Donations.Application.UseCases.Donations.GetDonationById;
using Fcs.Donations.Application.UseCases.Donations.GetDonations;
using Fcs.Donations.WebApi.Extensions;
using Fcs.Donations.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fcs.Donations.WebApi.Controllers.v1;

[Authorize]
public sealed class DonationsController : BaseApiController
{
    public DonationsController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet]
    [Authorize(Roles = "Doador,GestorONG")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<DonationQueryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(
            new GetDonationsQuery(page, pageSize, ParseStatus(status), sortBy, sortDescending),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<PagedResponse<DonationQueryResponse>>.FromSuccess(result.Value));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Doador,GestorONG")]
    [ProducesResponseType(typeof(ApiResponse<DonationQueryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetDonationByIdQuery(id), cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<DonationQueryResponse>.FromSuccess(result.Value));
    }

    [HttpPost]
    [Authorize(Roles = "Doador")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<CreateDonationResponse>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status503ServiceUnavailable)]
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

    private static Domain.Donations.DonationStatus? ParseStatus(string? status)
    {
        return status?.ToLowerInvariant() switch
        {
            "pending" => Domain.Donations.DonationStatus.Pending,
            "processed" => Domain.Donations.DonationStatus.Processed,
            "failed" => Domain.Donations.DonationStatus.Failed,
            _ => null
        };
    }
}
