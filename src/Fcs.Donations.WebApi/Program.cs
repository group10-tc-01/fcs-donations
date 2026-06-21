using System.Diagnostics.CodeAnalysis;
using Fcs.Donations.Application.DependencyInjection;
using Fcs.Donations.Infrastructure.Auth.DependencyInjection;
using Fcs.Donations.Infrastructure.Http.DependencyInjection;
using Fcs.Donations.Infrastructure.Kafka.DependencyInjection;
using Fcs.Donations.Infrastructure.SqlServer.DependencyInjection;
using Fcs.Donations.WebApi.DependencyInjection;

namespace Fcs.Donations.WebApi;

[ExcludeFromCodeCoverage]
public class Program
{
    protected Program()
    {
    }

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddWebApi(builder.Configuration);
        builder.Services.AddApplication();
        builder.Services.AddSqlServerInfrastructure(builder.Configuration);
        builder.Services.AddHttpInfrastructure(builder.Configuration);
        builder.Services.AddKafkaInfrastructure(builder.Configuration);
        builder.Services.AddAuthInfrastructure(builder.Configuration);

        var app = builder.Build();
        app.UseWebApiPipeline();
        app.Run();
    }
}
