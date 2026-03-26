using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AprovaFlow.Infrastructure.Data.Configurations;

public class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("approval_steps");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.RequestTypeId).HasColumnName("request_type_id").IsRequired();
        builder.Property(s => s.Label).HasColumnName("label").HasMaxLength(100).IsRequired();
        builder.Property(s => s.StepOrder).HasColumnName("step_order").IsRequired();
        builder.Property(s => s.ApproverUserId).HasColumnName("approver_user_id");
        builder.Property(s => s.ApproverRole).HasColumnName("approver_role")
            .HasConversion<string>()
            .HasMaxLength(50);
        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");

        // Aprovador fixo (opcional)
        builder.HasOne(s => s.ApproverUser)
            .WithMany()
            .HasForeignKey(s => s.ApproverUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
