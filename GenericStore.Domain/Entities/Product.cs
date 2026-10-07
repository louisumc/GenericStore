using GenericStore.Domain.Exceptions;

namespace GenericStore.Domain.Entities;

public class Product
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public bool Active { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Product() { }

    public Product(Guid storeId, string name, string? description, decimal price, int stock)
    {
        if (storeId == Guid.Empty)
            throw new DomainException("StoreId é obrigatório.");

        ValidateName(name);
        ValidatePrice(price);
        ValidateStock(stock);

        Id = Guid.NewGuid();
        StoreId = storeId;
        Name = name.Trim();
        Description = description?.Trim();
        Price = price;
        Stock = stock;
        Active = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void Update(string name, string? description, decimal price, int stock)
    {
        ValidateName(name);
        ValidatePrice(price);
        ValidateStock(stock);

        Name = name.Trim();
        Description = description?.Trim();
        Price = price;
        Stock = stock;
        UpdatedAt = DateTime.UtcNow;   // regra: só atualiza aqui
    }

    public void Deactivate() => Active = false;
    public void Activate() => Active = true;

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do produto é obrigatório.");

        var trimmed = name.Trim();
        if (trimmed.Length < 3 || trimmed.Length > 150)
            throw new DomainException("Nome do produto deve ter entre 3 e 150 caracteres.");
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
            throw new DomainException("Preço deve ser maior que zero.");
    }

    private static void ValidateStock(int stock)
    {
        if (stock < 0)
            throw new DomainException("Estoque não pode ser negativo.");
    }
}