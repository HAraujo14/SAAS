using AprovaFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AprovaFlow.Infrastructure.Data.Configurations;

public class RequestTypeConfiguration : IEntityTypeConfiguration<RequestType>
{
    public void Configure(EntityTypeBuilder<RequestType> builder)
    {
        builder.ToTable("request_types");
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id).HasColumnName("id");
        builder.Property(rt => rt.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(rt => rt.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(rt => rt.Description).HasColumnName("description");
        builder.Property(rt => rt.Icon).HasColumnName("icon").HasMaxLength(50).HasDefaultValue("file");
        builder.Property(rt => rt.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(rt => rt.DeletedAt).HasColumnName("deleted_at");
        builder.Property(rt => rt.CreatedAt).HasColumnName("created_at");
        builder.Property(rt => rt.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(rt => rt.TenantId).HasDatabaseName("idx_request_types_tenant_id");

        builder.HasOne(rt => rt.Tenant)
            .WithMany(t => t.RequestTypes)
            .HasForeignKey(rt => rt.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(rt => rt.Fields)
            .WithOne(f => f.RequestType)
            .HasForeignKey(f => f.RequestTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(rt => rt.ApprovalSteps)
            .WithOne(s => s.RequestType)
            .HasForeignKey(s => s.RequestTypeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
