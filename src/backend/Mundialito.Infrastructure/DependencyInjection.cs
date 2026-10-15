using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Mundialito.Application.Contracts;
using Mundialito.Application.Features.Matches.Queries;
using Mundialito.Application.Features.Players.Queries;
using Mundialito.Application.Features.Teams.Queries;
using Mundialito.Infrastructure.Idempotency;
using Mundialito.Infrastructure.Persistence;
using Mundialito.Infrastructure.Persistence.Seeding;
using Mundialito.Infrastructure.Queries;
using Mundialito.Infrastructure.Repositories;

namespace Mundialito.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSql")
            ?? throw new InvalidOperationException("No se configuró la conexión PostgreSql.");

        services.AddSingleton(_ => NpgsqlDataSource.Create(connectionString));
        services.AddDbContext<MundialitoDbContext>((serviceProvider, options) =>
            options.UseNpgsql(serviceProvider.GetRequiredService<NpgsqlDataSource>()));
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<MundialitoDbSeeder>();
        services.AddScoped<IIdempotencyStore, IdempotencyStore>();
        services.AddScoped<ITeamQueries, TeamQueries>();
        services.AddScoped<IPlayerQueries, PlayerQueries>();
        services.AddScoped<IMatchQueries, MatchQueries>();

        return services;
    }

    public static async Task InitializeDatabaseAsync(
        this IServiceProvider services,
        CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MundialitoDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<MundialitoDbSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}

