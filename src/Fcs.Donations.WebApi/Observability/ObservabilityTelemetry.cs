using System.Diagnostics.CodeAnalysis;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Fcs.Donations.WebApi.Observability;

[ExcludeFromCodeCoverage]
public static class ObservabilityTelemetry
{
    public static ResourceBuilder CreateResourceBuilder(ObservabilitySettings settings, string environment)
    {
        return ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: settings.ServiceName,
                serviceVersion: typeof(ObservabilityTelemetry).Assembly.GetName().Version?.ToString() ?? "1.0.0",
                serviceNamespace: "FCS")
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = environment
            });
    }

    public static TracerProviderBuilder ConfigureTracing(
        this TracerProviderBuilder builder,
        ObservabilitySettings settings,
        ResourceBuilder resourceBuilder)
    {
        builder
            .SetResourceBuilder(resourceBuilder)
            .AddSource("Fcs.Donations")
            .AddAspNetCoreInstrumentation(options =>
            {
                options.Filter = httpContext =>
                    !httpContext.Request.Path.StartsWithSegments("/health") &&
                    !httpContext.Request.Path.StartsWithSegments("/metrics");
            })
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation();

        if (settings.EnableOtlpExporter && !string.IsNullOrWhiteSpace(settings.OtlpEndpoint))
        {
            builder.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri($"{settings.OtlpEndpoint}/v1/traces");
                options.Protocol = OtlpExportProtocol.HttpProtobuf;

                if (!string.IsNullOrWhiteSpace(settings.OtlpAuthHeader))
                {
                    options.Headers = $"Authorization={settings.OtlpAuthHeader}";
                }
            });
        }

        return builder;
    }

    public static MeterProviderBuilder ConfigureMetrics(
        this MeterProviderBuilder builder,
        ObservabilitySettings settings,
        ResourceBuilder resourceBuilder)
    {
        builder
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation()
            .AddPrometheusExporter();

        if (settings.EnableOtlpExporter && !string.IsNullOrWhiteSpace(settings.OtlpEndpoint))
        {
            builder.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri($"{settings.OtlpEndpoint}/v1/metrics");
                options.Protocol = OtlpExportProtocol.HttpProtobuf;

                if (!string.IsNullOrWhiteSpace(settings.OtlpAuthHeader))
                {
                    options.Headers = $"Authorization={settings.OtlpAuthHeader}";
                }
            });
        }

        return builder;
    }
}
