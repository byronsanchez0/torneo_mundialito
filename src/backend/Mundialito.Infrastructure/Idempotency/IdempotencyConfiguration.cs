using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mundialito.Infrastructure.Idempotency;

internal sealed class IdempotencyConfiguration : IEntityTypeConfiguration<IdempotencyRequest>
{
    public void Configure(EntityTypeBuilder<IdempotencyRequest> builder)
    {
        builder.ToTable("idempotency_requests");
        builder.HasKey(request => request.Key);
        builder.Property(request => request.Key).HasMaxLength(200);
        builder.Property(request => request.RequestHash).HasMaxLength(64).IsRequired();
        builder.Property(request => request.ResponseStatusCode).IsRequired();
        builder.Property(request => request.ResponseBody).HasColumnType("jsonb");
        builder.Property(request => request.ResponseContentType).HasMaxLength(100);
        builder.HasIndex(request => request.CreatedAtUtc);
    }
}
