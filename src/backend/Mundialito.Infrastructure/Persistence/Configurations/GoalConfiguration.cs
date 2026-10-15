using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mundialito.Domain.Entities;

namespace Mundialito.Infrastructure.Persistence.Configurations;

internal sealed class GoalConfiguration : IEntityTypeConfiguration<Goal>
{
    public void Configure(EntityTypeBuilder<Goal> builder)
    {
        builder.ToTable("goals");
        builder.HasKey(goal => goal.Id);

        builder.HasOne<Player>()
            .WithMany()
            .HasForeignKey(goal => goal.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Team>()
            .WithMany()
            .HasForeignKey(goal => goal.TeamId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex("MatchId", nameof(Goal.TeamId), nameof(Goal.Minute));
    }
}

