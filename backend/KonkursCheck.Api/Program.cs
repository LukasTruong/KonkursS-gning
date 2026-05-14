using Hangfire;
using Hangfire.PostgreSql;
using KonkursCheck.Api.Endpoints;
using KonkursCheck.Application.Services;
using KonkursCheck.Infrastructure;
using KonkursCheck.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<CvrIntegrationService>();
builder.Services.AddScoped<EnrichmentService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<PersonService>();
builder.Services.AddScoped<CompanyService>();

builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("Postgres"))));

builder.Services.AddHangfireServer();

builder.Services.AddCors(opts => opts.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:3000", "http://frontend:3000")
          .AllowAnyHeader()
          .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseCors();
app.UseHangfireDashboard("/hangfire");

var api = app.MapGroup("/api");
api.MapSearchEndpoints();
api.MapPersonEndpoints();
api.MapCompanyEndpoints();

app.Run();
