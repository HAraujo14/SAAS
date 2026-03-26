using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AprovaFlow.Infrastructure.Data.Configurations;

public class RequestConfiguration : IEntityTypeConfiguration<Request>
{
    public void Configure(EntityTypeBuilder<Request> builder)
    {
        builder.ToTable("requests");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.TenantId).HasColumnName("tenant_id").IsRequired();
        builder.Property(r => r.RequestTypeId).HasColumnName("request_type_id").IsRequired();
        builder.Property(r => r.RequesterId).HasColumnName("requester_id").IsRequired();
        builder.Property(r => r.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(r => r.Description).HasColumnName("description");
        builder.Property(r => r.Status).HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(RequestStatus.Draft);
        builder.Property(r => r.CurrentStepId).HasColumnName("current_step_id");
        builder.Property(r => r.SubmittedAt).HasColumnName("submitted_at");
        builder.Property(r => r.ResolvedAt).HasColumnName("resolved_at");
        builder.Property(r => r.DeletedAt).HasColumnName("deleted_at");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at");

        // Índices críticos para performance
        builder.HasIndex(r => new { r.TenantId, r.Status })
            .HasDatabaseName("idx_requests_tenant_status");
        builder.HasIndex(r => r.RequesterId)
            .HasDatabaseName("idx_requests_requester_id");
        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("idx_requests_tenant_id");

        // Relações
        builder.HasOne(r => r.Tenant)
            .WithMany(t => t.Requests)
            .HasForeignKey(r => r.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.RequestType)
            .WithMany(rt => rt.Requests)
            .HasForeignKey(r => r.RequestTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Requester)
            .WithMany(u => u.RequestsCreated)
            .HasForeignKey(r => r.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.CurrentStep)
            .WithMany()
            .HasForeignKey(r => r.CurrentStepId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(r => r.FieldValues)
            .WithOne(fv => fv.Request)
            .HasForeignKey(fv => fv.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Approvals)
            .WithOne(a => a.Request)
            .HasForeignKey(a => a.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Comments)
            .WithOne(c => c.Request)
            .HasForeignKey(c => c.RequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Attachments)
            .WithOne(att => att.Request)
            .HasForeignKey(att => att.RequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
