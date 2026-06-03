using dotnet_test.Controllers.Contracts;
using dotnet_test.Data;
using dotnet_test.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_test.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly AppDbContext _db;

    public PlayersController(AppDbContext db) => _db = db;

    // GET api/players
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlayerResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var players = await _db.Players
            .OrderBy(p => p.Id)
            .Select(p => MapToResponse(p))
            .ToListAsync();
        return Ok(players);
    }

    // GET api/players/{playerId}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByPlayerId(string id)
    {
        var player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerId == id);
        return player is null ? NotFound() : Ok(MapToResponse(player));
    }

    // POST api/players
    [HttpPost]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePlayerRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (await _db.Players.AnyAsync(p => p.PlayerId == request.PlayerId))
            return Conflict(new { error = $"Player '{request.PlayerId}' already exists." });

        var player = new PlayerProfile
        {
            PlayerId = request.PlayerId,
            DaysSinceJoined = request.DaysSinceJoined,
            MostBetSport = request.MostBetSport,
            FavouriteTeam = request.FavouriteTeam,
            MostBetType = request.MostBetType,
            AverageStake = request.AverageStake,
            LastLoginDaysAgo = request.LastLoginDaysAgo,
            RiskLevel = request.RiskLevel,
            IsSelfExcluded = request.IsSelfExcluded,
            CoolingOffUntilUtc = request.CoolingOffUntilUtc,
            JurisdictionCode = request.JurisdictionCode,
            IsRecommendationRestricted = request.IsRecommendationRestricted,
            CreatedAt = DateTime.UtcNow
        };

        _db.Players.Add(player);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetByPlayerId), new { id = player.PlayerId }, MapToResponse(player));
    }

    // PUT api/players/{playerId}
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(PlayerResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePlayerRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerId == id);
        if (player is null) return NotFound();

        player.DaysSinceJoined = request.DaysSinceJoined;
        player.MostBetSport = request.MostBetSport;
        player.FavouriteTeam = request.FavouriteTeam;
        player.MostBetType = request.MostBetType;
        player.AverageStake = request.AverageStake;
        player.LastLoginDaysAgo = request.LastLoginDaysAgo;
        player.RiskLevel = request.RiskLevel;
        player.IsSelfExcluded = request.IsSelfExcluded;
        player.CoolingOffUntilUtc = request.CoolingOffUntilUtc;
        player.JurisdictionCode = request.JurisdictionCode;
        player.IsRecommendationRestricted = request.IsRecommendationRestricted;
        player.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Ok(MapToResponse(player));
    }

    // DELETE api/players/{playerId}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerId == id);
        if (player is null) return NotFound();

        _db.Players.Remove(player);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static PlayerResponse MapToResponse(PlayerProfile p) => new()
    {
        Id = p.Id,
        PlayerId = p.PlayerId,
        DaysSinceJoined = p.DaysSinceJoined,
        MostBetSport = p.MostBetSport,
        FavouriteTeam = p.FavouriteTeam,
        MostBetType = p.MostBetType,
        AverageStake = p.AverageStake,
        LastLoginDaysAgo = p.LastLoginDaysAgo,
        RiskLevel = p.RiskLevel,
        IsSelfExcluded = p.IsSelfExcluded,
        CoolingOffUntilUtc = p.CoolingOffUntilUtc,
        JurisdictionCode = p.JurisdictionCode,
        IsRecommendationRestricted = p.IsRecommendationRestricted,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };
}
