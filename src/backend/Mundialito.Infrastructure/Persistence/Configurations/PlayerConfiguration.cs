using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mundialito.Domain.Entities;

namespace Mundialito.Infrastructure.Persistence.Configurations;

internal sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players");
        builder.HasKey(player => player.Id);
        builder.Property(player => player.FirstName).HasMaxLength(80).IsRequired();
        builder.Property(player => player.LastName).HasMaxLength(80).IsRequired();
        builder.Property(player => player.Position).HasMaxLength(40).IsRequired();

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(player => player.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(player => new { player.TeamId, player.ShirtNumber }).IsUnique();
        builder.HasIndex(player => new { player.LastName, player.FirstName, player.Id });
    }
}

