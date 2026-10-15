using Microsoft.EntityFrameworkCore;
using Mundialito.Domain.Entities;

namespace Mundialito.Infrastructure.Persistence;

internal sealed class MundialitoDbContext(DbContextOptions<MundialitoDbContext> options)
    : DbContext(options)
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MundialitoDbContext).Assembly);
    }
}

