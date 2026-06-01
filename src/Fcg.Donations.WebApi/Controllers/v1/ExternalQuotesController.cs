using Fcg.Donations.Application.Abstractions.ExternalServices;
using Fcg.Donations.WebApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Donations.WebApi.Controllers.v1;

public sealed class ExternalQuotesController : BaseApiController
{
    public ExternalQuotesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpGet("zen")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetZen(
        [FromServices] IExternalQuoteClient externalQuoteClient,
        CancellationToken cancellationToken)
    {
        var quote = await externalQuoteClient.GetZenAsync(cancellationToken);
        return Ok(ApiResponse<string>.FromSuccess(quote));
    }
}
