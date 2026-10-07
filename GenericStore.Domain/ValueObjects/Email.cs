using GenericStore.Domain.Exceptions;
using System.Text.RegularExpressions;

public sealed class Email : IEquatable<Email>
{
    private static readonly Regex Pattern =
    new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string Value { get; }

    private Email() { Value = string.Empty; }
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("E-mail é obrigatório.");

        var normalized = value.Trim().ToLowerInvariant();

        if (!Pattern.IsMatch(normalized))
            throw new DomainException("E-mail inválido.");

        Value = normalized;
    }

    public bool Equals(Email? other) =>
        other is not null && Value == other.Value;

    public override bool Equals(object? obj) =>
        obj is Email other && Equals(other);

    public override int GetHashCode() => Value.GetHashCode();

    public override string ToString() => Value;

    // Operadores (opcional, mas elegante)
    public static bool operator ==(Email? a, Email? b) =>
        a is null ? b is null : a.Equals(b);

    public static bool operator !=(Email? a, Email? b) => !(a == b);
}