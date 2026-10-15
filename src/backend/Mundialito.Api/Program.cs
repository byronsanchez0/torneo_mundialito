using Microsoft.EntityFrameworkCore;
using Mundialito.Application.Contracts;
using Mundialito.Infrastructure;
using Mundialito.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy
        .WithOrigins(builder.Configuration["WebOrigin"] ?? "http://localhost:3001")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (builder.Configuration.GetValue<bool>("InitializeDatabase"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<MundialitoDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseCors();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/teams", (ITeamRepository repository, CancellationToken cancellationToken) =>
    repository.GetAllAsync(cancellationToken));

app.MapGet("/api/players", (IPlayerRepository repository, CancellationToken cancellationToken) =>
    repository.GetAllAsync(cancellationToken));

app.MapGet("/api/matches", (IMatchRepository repository, CancellationToken cancellationToken) =>
    repository.GetAllAsync(cancellationToken));

app.Run();

