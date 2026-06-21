using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Infrastructure.SqlServer.Persistence;
using Fcs.Donations.WebApi.Middlewares;
using Microsoft.EntityFrameworkCore;

namespace Fcs.Donations.WebApi.Extensions;

[ExcludeFromCodeCoverage]
public static class ApiBuilderExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        using var dbContext = scope.ServiceProvider.GetRequiredService<FcsDonationsDbContext>();

        dbContext.Database.Migrate();
    }

    public static void UseGlobalCorrelationId(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalCorrelationIdMiddleware>();
    }

    public static void UseCustomerExceptionHandler(this IApplicationBuilder app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
    }

    public static void UseRequestFlowLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestFlowLoggingMiddleware>();
    }
}
