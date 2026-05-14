using KonkursCheck.Infrastructure.Caching;
using KonkursCheck.Infrastructure.Cvr;
using KonkursCheck.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace KonkursCheck.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("Postgres")));

        var redisConn = ConnectionMultiplexer.Connect(config.GetConnectionString("Redis")!);
        services.AddSingleton<IConnectionMultiplexer>(redisConn);
        services.AddSingleton<IRedisCacheService, RedisCacheService>();

        services.AddHttpClient<ICvrElasticClient, CvrElasticClient>(client =>
        {
            client.BaseAddress = new Uri(config["Cvr:BaseUrl"]!);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }
}
