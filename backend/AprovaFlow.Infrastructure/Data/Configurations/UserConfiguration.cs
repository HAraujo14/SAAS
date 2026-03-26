using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AprovaFlow.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id).HasColumnName("id");
        builder.Property(u => u.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(u => u.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(254).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired();
        builder.Property(u => u.Role).HasColumnName("role")
            .HasConversion<string>()  // armazena como string legível (ex: "Admin")
            .HasMaxLength(50)
            .HasDefaultValue(UserRole.Collaborator);
        builder.Property(u => u.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(u => u.LastLoginAt).HasColumnName("last_login_at");
        builder.Property(u => u.DeletedAt).HasColumnName("deleted_at");
        builder.Property(u => u.CreatedAt).HasColumnName("created_at");
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        // Email único por tenant (não globalmente)
        builder.HasIndex(u => new { u.Email, u.TenantId })
            .IsUnique()
            .HasDatabaseName("idx_users_email_tenant");

        // Índice para filtrar por tenant (query frequente)
        builder.HasIndex(u => u.TenantId).HasDatabaseName("idx_users_tenant_id");

        // Relações
        builder.HasOne(u => u.Tenant)
            .WithMany(t => t.Users)
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Refresh tokens — cascade delete: ao apagar utilizador, apaga tokens
        builder.HasMany(u => u.RefreshTokens)
            .WithOne(rt => rt.User)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
