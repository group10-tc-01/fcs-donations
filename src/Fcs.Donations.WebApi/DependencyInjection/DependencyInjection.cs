using Asp.Versioning;
using Fcs.Donations.WebApi.Middlewares;
using Fcs.Donations.WebApi.Observability;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using System.Text.Json.Serialization;

namespace Fcs.Donations.WebApi.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Fcs.Donations API",
                Version = "v1"
            });
        });

        services.AddHealthChecks();
        services.AddRouting(options => options.LowercaseUrls = true);

        services.AddObservability(configuration);
        services.AddSerilogLogging(configuration);

        return services;
    }

    public static WebApplication UseWebApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapHealthChecks("/health", new HealthCheckOptions());
        app.MapPrometheusScrapingEndpoint("/metrics");
        return app;
    }

    private static IServiceCollection AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new ObservabilityOptions();
        configuration.GetSection(ObservabilityOptions.SectionName).Bind(options);

        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(options.ServiceName, serviceNamespace: "FCS")
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environment
            });

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(options.ServiceName, serviceNamespace: "FCS"))
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.Filter = httpContext =>
                            !httpContext.Request.Path.StartsWithSegments("/health") &&
                            !httpContext.Request.Path.StartsWithSegments("/metrics");
                    })
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation();

                if (options.EnableOtlpExporter && !string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    builder.AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = new Uri($"{options.OtlpEndpoint}/v1/traces");
                        exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                        if (!string.IsNullOrWhiteSpace(options.OtlpAuthHeader))
                        {
                            exporterOptions.Headers = $"Authorization={options.OtlpAuthHeader}";
                        }
                    });
                }
            })
            .WithMetrics(builder =>
            {
                builder
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddPrometheusExporter();

                if (options.EnableOtlpExporter && !string.IsNullOrWhiteSpace(options.OtlpEndpoint))
                {
                    builder.AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = new Uri($"{options.OtlpEndpoint}/v1/metrics");
                        exporterOptions.Protocol = OtlpExportProtocol.HttpProtobuf;
                        if (!string.IsNullOrWhiteSpace(options.OtlpAuthHeader))
                        {
                            exporterOptions.Headers = $"Authorization={options.OtlpAuthHeader}";
                        }
                    });
                }
            });

        return services;
    }

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var options = new ObservabilityOptions();
        configuration.GetSection(ObservabilityOptions.SectionName).Bind(options);
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", options.ServiceName)
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Console();

        if (options.EnableOtlpExporter && !string.IsNullOrWhiteSpace(options.OtlpEndpoint))
        {
            loggerConfiguration.WriteTo.OpenTelemetry(otlpOptions =>
            {
                otlpOptions.Endpoint = $"{options.OtlpEndpoint}/v1/logs";
                otlpOptions.Protocol = OtlpProtocol.HttpProtobuf;
                if (!string.IsNullOrWhiteSpace(options.OtlpAuthHeader))
                {
                    otlpOptions.Headers = new Dictionary<string, string>
                    {
                        ["Authorization"] = options.OtlpAuthHeader
                    };
                }
                otlpOptions.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = options.ServiceName,
                    ["service.namespace"] = "FCS",
                    ["deployment.environment"] = environment
                };
            });
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog();
        });

        return services;
    }
}
