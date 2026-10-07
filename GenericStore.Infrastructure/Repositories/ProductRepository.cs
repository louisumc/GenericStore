using GenericStore.Application.Abstractions;
using GenericStore.Domain.Entities;
using GenericStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GenericStore.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly GenericStoreDbContext _db;

    public ProductRepository(GenericStoreDbContext db) => _db = db;

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct) =>
        await _db.Products
            .AsNoTracking()
            .Where(p => p.Active)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Product>> GetActiveByStoreIdAsync(Guid storeId, CancellationToken ct) =>
        await _db.Products
            .AsNoTracking()
            .Where(p => p.Active && p.StoreId == storeId)
            .ToListAsync(ct);

    public async Task AddAsync(Product product, CancellationToken ct)
    {
        await _db.Products.AddAsync(product, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }
}