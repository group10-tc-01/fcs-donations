using Fcg.Donations.Domain;
using Fcg.Donations.WebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fcg.Donations.WebApi.Extensions;

public static class ErrorExtensions
{
    public static IActionResult ToActionResult(this Error error) =>
        error.Type switch
        {
            ErrorType.NotFound    => new NotFoundObjectResult(ApiResponse<string>.FromFailure(error.Message)),
            ErrorType.Conflict    => new ConflictObjectResult(ApiResponse<string>.FromFailure(error.Message)),
            ErrorType.Validation  => new BadRequestObjectResult(ApiResponse<string>.FromFailure(error.Message)),
            _                     => new ObjectResult(ApiResponse<string>.FromFailure(error.Message)) { StatusCode = StatusCodes.Status500InternalServerError }
        };
}
