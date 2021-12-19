using System.Collections.Immutable;

namespace SwIpToGemp;

record CardDatabase
{
    public ImmutableArray<Card> Cards { get; init; } = ImmutableArray<Card>.Empty;
}
