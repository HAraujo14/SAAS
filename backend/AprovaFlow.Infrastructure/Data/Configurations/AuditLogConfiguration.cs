using AprovaFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AprovaFlow.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");
        builder.HasKey(al => al.Id);

        builder.Property(al => al.Id).HasColumnName("id");
        builder.Property(al => al.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(al => al.EntityType).HasColumnName("entity_type").HasMaxLength(50).IsRequired();
        builder.Property(al => al.EntityId).HasColumnName("entity_id").IsRequired();
        builder.Property(al => al.Action).HasColumnName("action").HasMaxLength(100).IsRequired();
        builder.Property(al => al.ActorId).HasColumnName("actor_id");
        builder.Property(al => al.ActorName).HasColumnName("actor_name").HasMaxLength(200);
        builder.Property(al => al.Payload).HasColumnName("payload").HasColumnType("jsonb");
        builder.Property(al => al.CreatedAt).HasColumnName("created_at");

        // AuditLog é imutável — sem UpdatedAt
        builder.HasIndex(al => new { al.EntityType, al.EntityId })
            .HasDatabaseName("idx_audit_logs_entity");
        builder.HasIndex(al => al.TenantId)
            .HasDatabaseName("idx_audit_logs_tenant_id");

        builder.HasOne(al => al.Actor)
            .WithMany()
            .HasForeignKey(al => al.ActorId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
