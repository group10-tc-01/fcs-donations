using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi;

namespace Fcs.Donations.WebApi.Swagger;

[ExcludeFromCodeCoverage]
public static class SwaggerDependencyInjection
{
    public static IServiceCollection AddDonationsSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Fcs.Donations API",
                Version = "v1.0"
            });

            options.AddSecurityDefinition(SwaggerConstants.BearerSecurityScheme, new OpenApiSecurityScheme
            {
                Description = "JWT Bearer token issued by Keycloak. Use: Bearer {accessToken}.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            options.OperationFilter<SwaggerAuthorizationOperationFilter>();
            options.OperationFilter<SwaggerEndpointDocumentationOperationFilter>();
        });

        return services;
    }
}
