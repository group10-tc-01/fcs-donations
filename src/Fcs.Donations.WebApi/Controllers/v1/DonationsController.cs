using Fcs.Donations.Application.Common.Pagination;
using Fcs.Donations.Application.UseCases.Donations.CreateDonation;
using Fcs.Donations.Application.UseCases.Donations.GetAdminDonations;
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
    [Authorize(Roles = "Doador")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<DonationQueryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var parsedStatus = status?.ToLowerInvariant() switch
        {
            "pending" => Domain.Donations.DonationStatus.Pending,
            "processed" => Domain.Donations.DonationStatus.Processed,
            "failed" => Domain.Donations.DonationStatus.Failed,
            _ => (Domain.Donations.DonationStatus?)null
        };

        var result = await Mediator.Send(
            new GetDonationsQuery(page, pageSize, parsedStatus, sortBy, sortDescending),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<PagedResponse<DonationQueryResponse>>.FromSuccess(result.Value));
    }

    [HttpGet("admin")]
    [Authorize(Roles = "GestorONG")]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<DonationQueryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAdmin(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        CancellationToken cancellationToken = default)
    {
        var parsedStatus = status?.ToLowerInvariant() switch
        {
            "pending" => Domain.Donations.DonationStatus.Pending,
            "processed" => Domain.Donations.DonationStatus.Processed,
            "failed" => Domain.Donations.DonationStatus.Failed,
            _ => (Domain.Donations.DonationStatus?)null
        };

        var result = await Mediator.Send(
            new GetAdminDonationsQuery(page, pageSize, parsedStatus, sortBy, sortDescending),
            cancellationToken);

        if (result.IsFailure)
        {
            return result.Error.ToActionResult();
        }

        return Ok(ApiResponse<PagedResponse<DonationQueryResponse>>.FromSuccess(result.Value));
    }

    [HttpPost]
    [Authorize(Roles = "Doador")]
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
