using System.Text.RegularExpressions;
using GenericStore.Domain.Exceptions;

namespace GenericStore.Domain.ValueObjects;

public sealed class Slug
{
    private static readonly Regex Pattern =
        new(@"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public string Value { get; }

    private Slug() { Value = string.Empty; }

    public Slug(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Slug é obrigatório.");

        var normalized = value.Trim().ToLowerInvariant();

        if (!Pattern.IsMatch(normalized))
            throw new DomainException("Slug inválido. Use apenas letras minúsculas, números e hífens.");

        Value = normalized;
    }

    public override string ToString() => Value;
}