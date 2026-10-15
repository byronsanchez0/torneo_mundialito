using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mundialito.Domain.Entities;

namespace Mundialito.Infrastructure.Persistence.Configurations;

internal sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.ToTable("teams");
        builder.HasKey(team => team.Id);
        builder.Property(team => team.Name).HasMaxLength(100).IsRequired();
        builder.Property(team => team.Coach).HasMaxLength(100);
        builder.HasIndex(team => team.Name).IsUnique();
    }
}

