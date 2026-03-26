using AprovaFlow.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AprovaFlow.Infrastructure.Data.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.RequestId).HasColumnName("request_id").IsRequired();
        builder.Property(c => c.AuthorId).HasColumnName("author_id").IsRequired();
        builder.Property(c => c.Content).HasColumnName("content").IsRequired();
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(c => c.RequestId).HasDatabaseName("idx_comments_request_id");

        builder.HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
