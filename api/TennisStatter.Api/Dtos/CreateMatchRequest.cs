using TennisStatter.Api.Models;

namespace TennisStatter.Api.Dtos;

public class CreateMatchRequest
{
    public MatchMode Mode { get; set; }
}
