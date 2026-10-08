using GenericStore.Domain.Exceptions;
using GenericStore.Domain.ValueObjects;

namespace GenericStore.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Email Email { get; private set; }

    public string PasswordHash { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navegação: um User tem várias Stores
    private readonly List<Store> _stores = new();
    public IReadOnlyCollection<Store> Stores => _stores.AsReadOnly();

    // EF Core precisa deste construtor
    private User() { }

    public User(string name, Email email, string passwordHash)
    {
        ValidateName(name);

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash é obrigatório.");

        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name;
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome é obrigatório.");

        var trimmed = name.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 100)
            throw new DomainException("Nome deve ter entre 3 e 100 caracteres.");
    }
}