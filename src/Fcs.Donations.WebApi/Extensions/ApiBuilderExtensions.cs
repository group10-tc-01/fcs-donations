using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.WebApi.Middlewares;

namespace Fcs.Donations.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class ApiBuilderExtensions
{
    public static void UseGlobalCorrelationId(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalCorrelationIdMiddleware>();
    }

    public static void UseCustomerExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
