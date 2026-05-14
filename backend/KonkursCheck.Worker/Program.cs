using Hangfire;
using Hangfire.PostgreSql;
using KonkursCheck.Application.Services;
using KonkursCheck.Infrastructure;
using KonkursCheck.Worker.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<CvrIntegrationService>();
builder.Services.AddScoped<EnrichmentService>();
builder.Services.AddScoped<CvrSyncJob>();

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Postgres"))));

builder.Services.AddHangfireServer();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    RecurringJob.AddOrUpdate<CvrSyncJob>(
        "cvr-daily-sync",
        job => job.ExecuteAsync(CancellationToken.None),
        "0 3 * * *");
}

host.Run();
