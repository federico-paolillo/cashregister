using NUlid;
using NUlid.Rng;

namespace Cashregister.Domain;

public sealed record Identifier(string Value)
{
    private static readonly MonotonicUlidRng Rng = new();

    public static Identifier New()
    {
        Ulid ulid = Ulid.NewUlid(Rng);
        string ulidString = ulid.ToString();

        return new Identifier(ulidString);
    }

    public static Identifier From(string rawIdentifier)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawIdentifier);

        return new Identifier(rawIdentifier);
    }
}