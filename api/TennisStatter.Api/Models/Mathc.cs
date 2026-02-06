namespace TennisStatter.Api.Models;

public enum MatchMode
{
    Singles = 0,
    Doubles = 1
}

public class Match
{
    public Guid Id { get; set; }
    public MatchMode Mode { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
