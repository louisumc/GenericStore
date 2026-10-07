using GenericStore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GenericStore.Infrastructure.Persistence;

public class GenericStoreDbContext : DbContext
{
    public GenericStoreDbContext(DbContextOptions<GenericStoreDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GenericStoreDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}