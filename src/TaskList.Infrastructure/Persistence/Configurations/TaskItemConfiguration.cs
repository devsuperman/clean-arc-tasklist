using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskList.Domain.Categories;
using TaskList.Domain.Tasks;

namespace TaskList.Infrastructure.Persistence.Configurations;

internal sealed class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    private const int EnumMaxLength = 20;

    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");
        builder.HasKey(task => task.Id);
        builder.Property(task => task.Id).ValueGeneratedNever();

        builder.Property(task => task.Title).HasMaxLength(TaskItem.TitleMaxLength).IsRequired();
        builder.Property(task => task.Description).HasMaxLength(TaskItem.DescriptionMaxLength);

        // Enums as strings keep the database readable.
        builder.Property(task => task.Status).HasConversion<string>().HasMaxLength(EnumMaxLength).IsRequired();
        builder.Property(task => task.Priority).HasConversion<string>().HasMaxLength(EnumMaxLength).IsRequired();

        builder.Property(task => task.DueDate).HasColumnType("date");
        builder.Property(task => task.CreatedAt).IsRequired();

        // BR-C04 at database level: a category with tasks cannot be deleted.
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(task => task.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(task => task.CategoryId);
        builder.HasIndex(task => task.Status);
        builder.HasIndex(task => task.DueDate);
    }
}
