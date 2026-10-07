using GenericStore.Domain.Entities;
using GenericStore.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GenericStore.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.ToTable("Stores");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Slug)
            .HasConversion(
                vo => vo.Value,
                s => new Slug(s))
            .HasColumnName("Slug")
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(s => s.Slug).IsUnique();

        builder.Property(s => s.Active).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasMany(s => s.Products)
            .WithOne()
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}