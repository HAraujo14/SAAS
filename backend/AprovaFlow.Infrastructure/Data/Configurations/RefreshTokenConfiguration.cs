using AprovaFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AprovaFlow.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id).HasColumnName("id");
        builder.Property(rt => rt.UserId).HasColumnName("user_id").IsRequired();
        builder.Property(rt => rt.TokenHash).HasColumnName("token_hash").IsRequired();
        builder.Property(rt => rt.ExpiresAt).HasColumnName("expires_at").IsRequired();
        builder.Property(rt => rt.IsUsed).HasColumnName("is_used").HasDefaultValue(false);
        builder.Property(rt => rt.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false);
        builder.Property(rt => rt.CreatedByIp).HasColumnName("created_by_ip").HasMaxLength(45);
        builder.Property(rt => rt.CreatedAt).HasColumnName("created_at");
        builder.Property(rt => rt.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(rt => rt.TokenHash).HasDatabaseName("idx_refresh_tokens_hash");
        builder.HasIndex(rt => rt.UserId).HasDatabaseName("idx_refresh_tokens_user_id");
    }
}
