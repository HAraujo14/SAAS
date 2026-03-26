using AprovaFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AprovaFlow.Infrastructure.Data.Configurations;

public class ApprovalConfiguration : IEntityTypeConfiguration<Approval>
{
    public void Configure(EntityTypeBuilder<Approval> builder)
    {
        builder.ToTable("approvals");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.RequestId).HasColumnName("request_id").IsRequired();
        builder.Property(a => a.ApprovalStepId).HasColumnName("approval_step_id").IsRequired();
        builder.Property(a => a.ApproverId).HasColumnName("approver_id").IsRequired();
        builder.Property(a => a.Decision).HasColumnName("decision")
            .HasConversion<string>()
            .HasMaxLength(50);
        builder.Property(a => a.Comment).HasColumnName("comment");
        builder.Property(a => a.DecidedAt).HasColumnName("decided_at");
        builder.Property(a => a.DelegatedToUserId).HasColumnName("delegated_to_user_id");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => a.RequestId).HasDatabaseName("idx_approvals_request_id");
        builder.HasIndex(a => a.ApproverId).HasDatabaseName("idx_approvals_approver_id");

        builder.HasOne(a => a.Approver)
            .WithMany(u => u.Approvals)
            .HasForeignKey(a => a.ApproverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.ApprovalStep)
            .WithMany(s => s.Approvals)
            .HasForeignKey(a => a.ApprovalStepId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.DelegatedToUser)
            .WithMany()
            .HasForeignKey(a => a.DelegatedToUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
