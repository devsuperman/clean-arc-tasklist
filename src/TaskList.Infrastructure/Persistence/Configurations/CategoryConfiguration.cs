using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskList.Domain.Categories;

namespace TaskList.Infrastructure.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Id).ValueGeneratedNever();

        builder.Property(category => category.Name).HasMaxLength(Category.NameMaxLength).IsRequired();
        builder.Property(category => category.Description).HasMaxLength(Category.DescriptionMaxLength);
        builder.Property(category => category.Color).HasMaxLength(Category.ColorLength);
        builder.Property(category => category.CreatedAt).IsRequired();

        // BR-C01 (case-insensitive unique name) is backed by a unique index on lower(name),
        // created with raw SQL in the InitialCreate migration because EF Core cannot model expression indexes.
    }
}
