using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mundialito.Application.Contracts;
using Mundialito.Infrastructure.Persistence;
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

        services.AddDbContext<MundialitoDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();

        return services;
    }
}

