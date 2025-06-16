namespace CashRegister.Domain;

public sealed record Identifier(string Value)
{
    public static Identifier New()
    {
        var ulid = Ulid.NewUlid();
        var ulidString = ulid.ToString();
        
        return new Identifier(ulidString);
    }

    public static Identifier From(string rawIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawIdentifier);

        return new Identifier(rawIdentifier);
    }
}