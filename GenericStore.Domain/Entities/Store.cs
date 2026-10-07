using GenericStore.Domain.Exceptions;
using GenericStore.Domain.ValueObjects;

namespace GenericStore.Domain.Entities;

public class Store
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public Slug Slug { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

    private Store() { }

    public Store(Guid userId, string name, Slug slug)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId é obrigatório.");

        ValidateName(name);

        Id = Guid.NewGuid();
        UserId = userId;
        Name = name.Trim();
        Slug = slug;
        Active = true;               // regra: sempre nasce ativa
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateName(string name)
    {
        ValidateName(name);
        Name = name.Trim();
    }

    public void Deactivate() => Active = false;
    public void Activate() => Active = true;

    public bool CanReceiveProducts() => Active;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da loja é obrigatório.");

        var trimmed = name.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 100)
            throw new DomainException("Nome da loja deve ter entre 3 e 100 caracteres.");
    }
}