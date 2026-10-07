using GenericStore.Application.Abstractions;
using GenericStore.Domain.Entities;
using GenericStore.Domain.ValueObjects;
using GenericStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GenericStore.Infrastructure.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly GenericStoreDbContext _db;

    public StoreRepository(GenericStoreDbContext db) => _db = db;

    public async Task<Store?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Stores.FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Store>> GetAllAsync(CancellationToken ct) =>
        await _db.Stores.AsNoTracking().ToListAsync(ct);

    public async Task<IReadOnlyList<Store>> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        await _db.Stores
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .ToListAsync(ct);

    public async Task<bool> SlugExistsAsync(Slug slug, CancellationToken ct) =>
        await _db.Stores.AnyAsync(s => s.Slug == slug, ct);

    public async Task AddAsync(Store store, CancellationToken ct)
    {
        await _db.Stores.AddAsync(store, ct);
        await _db.SaveChangesAsync(ct);
    }
}