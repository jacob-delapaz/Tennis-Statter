using Microsoft.AspNetCore.Mvc;
using TennisStatter.Api.Data;
using TennisStatter.Api.Dtos;
using TennisStatter.Api.Models;

namespace TennisStatter.Api.Controllers;

[ApiController]
[Route("matches")]
public class MatchesController : ControllerBase
{
    [HttpPost]
    public ActionResult<object> CreateMatch([FromBody] CreateMatchRequest request)
    {
        var match = new Match
        {
            Id = Guid.NewGuid(),
            Mode = request.Mode,
            CreatedAtUtc = DateTime.UtcNow
        };

        InMemoryMatchStore.Add(match);

        return Ok(new { matchId = match.Id });
    }

    [HttpGet("{id:guid}")]
    public ActionResult<Match> GetMatch([FromRoute] Guid id)
    {
        var match = InMemoryMatchStore.Get(id);
        if (match is null) return NotFound();
        return Ok(match);
    }
}
