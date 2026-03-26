using AprovaFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AprovaFlow.Infrastructure.Data.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("attachments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.RequestId).HasColumnName("request_id").IsRequired();
        builder.Property(a => a.UploadedById).HasColumnName("uploaded_by_id").IsRequired();
        builder.Property(a => a.FileName).HasColumnName("file_name").HasMaxLength(255).IsRequired();
        builder.Property(a => a.StoragePath).HasColumnName("storage_path").IsRequired();
        builder.Property(a => a.MimeType).HasColumnName("mime_type").HasMaxLength(100).IsRequired();
        builder.Property(a => a.SizeBytes).HasColumnName("size_bytes").IsRequired();
        builder.Property(a => a.DeletedAt).HasColumnName("deleted_at");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at");
        builder.Property(a => a.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(a => a.RequestId).HasDatabaseName("idx_attachments_request_id");

        builder.HasOne(a => a.UploadedBy)
            .WithMany(u => u.Attachments)
            .HasForeignKey(a => a.UploadedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
