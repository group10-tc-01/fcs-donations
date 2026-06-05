using fcs.Donations.Application.DependencyInjection;
using fcs.Donations.Infrastructure.Auth.DependencyInjection;
using fcs.Donations.Infrastructure.Http.DependencyInjection;
using fcs.Donations.Infrastructure.Kafka.DependencyInjection;
using fcs.Donations.Infrastructure.SqlServer.DependencyInjection;
using fcs.Donations.WebApi.DependencyInjection;

namespace fcs.Donations.WebApi;

public class Program
{
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
