using TennisStatter.Api.Models;

namespace TennisStatter.Api.Data;

public static class InMemoryMatchStore
{
    private static readonly Dictionary<Guid, Match> _matches = new();

    public static Match Add(Match match)
    {
        _matches[match.Id] = match;
        return match;
    }

    public static Match? Get(Guid id)
    {
        _matches.TryGetValue(id, out var match);
        return match;
    }
}
