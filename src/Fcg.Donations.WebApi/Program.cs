using Fcg.Donations.Application.DependencyInjection;
using Fcg.Donations.Infrastructure.Auth.DependencyInjection;
using Fcg.Donations.Infrastructure.Http.DependencyInjection;
using Fcg.Donations.Infrastructure.Kafka.DependencyInjection;
using Fcg.Donations.Infrastructure.SqlServer.DependencyInjection;
using Fcg.Donations.WebApi.DependencyInjection;

namespace Fcg.Donations.WebApi;

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
