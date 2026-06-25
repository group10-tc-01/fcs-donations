using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Fcs.Donations.WebApi.Filters;
using Fcs.Donations.WebApi.Observability;
using Fcs.Donations.WebApi.Settings;
using Fcs.Donations.WebApi.Swagger;
using Microsoft.AspNetCore.OData;
using Serilog;
using Serilog.Sinks.OpenTelemetry;

namespace Fcs.Donations.WebApi.DependencyInjection;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers()
            .AddOData(options => options.Select().Filter().OrderBy().Count().SetMaxTop(100))
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddEndpointsApiExplorer();
        services.AddDonationsSwagger();
        services.AddCorsConfiguration(configuration);
        services.AddVersioning();
        services.AddFilters();
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddObservabilitySettings(configuration);
        services.AddObservability(configuration);
        services.AddSerilogLogging(configuration);

        return services;
    }

    private static void AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(CorsSettings.SectionName)
            .Get<CorsSettings>()
            ?? new CorsSettings { AllowedOrigins = ["http://localhost:4200", "http://127.0.0.1:4200"] };

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(settings.AllowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private static void AddVersioning(this IServiceCollection services)
    {
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
    }

    private static void AddFilters(this IServiceCollection services)
    {
        services.AddMvc(options =>
        {
            options.Filters.Add<TrimStringsActionFilter>();
        });
    }

    private static void AddObservability(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = GetObservabilitySettings(configuration);
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";
        var resourceBuilder = ObservabilityTelemetry.CreateResourceBuilder(settings, environment);

        services.AddOpenTelemetry()
            .WithTracing(builder => builder.ConfigureTracing(settings, resourceBuilder))
            .WithMetrics(builder => builder.ConfigureMetrics(settings, resourceBuilder));
    }

    private static void AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = GetObservabilitySettings(configuration);
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Application", "Fcs.Donations")
            .Enrich.WithProperty("Environment", environment)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}");

        if (settings.EnableOtlpExporter && !string.IsNullOrWhiteSpace(settings.OtlpEndpoint))
        {
            loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = $"{settings.OtlpEndpoint}/otlp/v1/logs";
                options.Protocol = OtlpProtocol.HttpProtobuf;

                if (!string.IsNullOrWhiteSpace(settings.OtlpAuthHeader))
                {
                    options.Headers = new Dictionary<string, string>
                    {
                        ["Authorization"] = settings.OtlpAuthHeader
                    };
                }

                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = settings.ServiceName,
                    ["deployment.environment"] = environment
                };
            });
        }

        Log.Logger = loggerConfiguration.CreateLogger();

        Log.Information("Starting {Application} application", "Fcs.Donations");
        Log.Information("Environment: {Environment}", environment);

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        });
    }

    private static void AddObservabilitySettings(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ObservabilitySettings>()
            .Bind(configuration.GetRequiredSection(ObservabilitySettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
    }

    private static ObservabilitySettings GetObservabilitySettings(IConfiguration configuration)
    {
        return configuration
            .GetRequiredSection(ObservabilitySettings.SectionName)
            .Get<ObservabilitySettings>()
            ?? throw new InvalidOperationException("Observability settings must be configured.");
    }
}
