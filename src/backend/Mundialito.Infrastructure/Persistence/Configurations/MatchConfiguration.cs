using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mundialito.Domain.Entities;

namespace Mundialito.Infrastructure.Persistence.Configurations;

internal sealed class MatchConfiguration : IEntityTypeConfiguration<Match>
{
    public void Configure(EntityTypeBuilder<Match> builder)
    {
        builder.ToTable("matches");
        builder.HasKey(match => match.Id);
        builder.Property(match => match.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(match => match.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(match => match.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(match => match.Goals)
            .WithOne()
            .HasForeignKey("MatchId")
            .OnDelete(DeleteBehavior.Cascade);

        var goalsNavigation = builder.Metadata.FindNavigation(nameof(Match.Goals));
        goalsNavigation?.SetField("_goals");
        goalsNavigation?.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(match => new { match.ScheduledAtUtc, match.Id });
    }
}

