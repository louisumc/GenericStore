using GenericStore.Application.Abstractions;
using GenericStore.Domain.Entities;
using GenericStore.Domain.ValueObjects;
using GenericStore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GenericStore.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly GenericStoreDbContext _db;

    public UserRepository(GenericStoreDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct) =>
        await _db.Users.AsNoTracking().ToListAsync(ct);

    public async Task<bool> EmailExistsAsync(Email email, CancellationToken ct) =>
        await _db.Users.AnyAsync(u => u.Email == email, ct);

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _db.Users.AddAsync(user, ct);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken ct) =>
    await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
}