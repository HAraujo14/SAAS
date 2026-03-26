using AprovaFlow.Core.Entities;
using AprovaFlow.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AprovaFlow.Infrastructure.Data.Configurations;

public class RequestFieldConfiguration : IEntityTypeConfiguration<RequestField>
{
    public void Configure(EntityTypeBuilder<RequestField> builder)
    {
        builder.ToTable("request_fields");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.RequestTypeId).HasColumnName("request_type_id").IsRequired();
        builder.Property(f => f.Label).HasColumnName("label").HasMaxLength(100).IsRequired();
        builder.Property(f => f.Placeholder).HasColumnName("placeholder").HasMaxLength(200);
        builder.Property(f => f.FieldType).HasColumnName("field_type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(FieldType.Text);
        builder.Property(f => f.IsRequired).HasColumnName("is_required").HasDefaultValue(true);
        builder.Property(f => f.Options).HasColumnName("options").HasColumnType("jsonb");
        builder.Property(f => f.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);
        builder.Property(f => f.CreatedAt).HasColumnName("created_at");
        builder.Property(f => f.UpdatedAt).HasColumnName("updated_at");
    }
}
