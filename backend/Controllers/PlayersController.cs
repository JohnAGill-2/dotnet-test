using dotnet_test.Controllers.Contracts;
using dotnet_test.Data;
using dotnet_test.Data.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace dotnet_test.Controllers;

[ApiController]
[Authorize]
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
        var query = _db.Players.AsQueryable();

        if (!IsAdmin())
        {
            var playerId = CurrentPlayerId();
            if (string.IsNullOrWhiteSpace(playerId))
            {
                return Forbid();
            }

            query = query.Where(p => p.PlayerId == playerId);
        }

        var players = await query
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
        if (!CanAccessPlayer(id))
        {
            return Forbid();
        }

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

        if (!IsAdmin())
        {
            var playerId = CurrentPlayerId();
            if (!string.Equals(request.PlayerId, playerId, StringComparison.Ordinal))
            {
                return Forbid();
            }
        }

        var duplicate = await _db.Players
            .AnyAsync(p => p.PlayerId == request.PlayerId);
        if (duplicate) return Conflict(new { error = "PlayerId already exists." });

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
            SportMix30d = request.SportMix30d,
            BetTypeMix30d = request.BetTypeMix30d,
            StakeStdDev30d = request.StakeStdDev30d,
            TimeOfDayFitScore = request.TimeOfDayFitScore,
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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdatePlayerRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        if (!CanAccessPlayer(id))
        {
            return Forbid();
        }

        var player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerId == id);
        if (player is null) return NotFound();

        var duplicate = await _db.Players
            .AnyAsync(p => p.PlayerId != id && p.PlayerId == request.PlayerId);
        if (duplicate) return Conflict(new { error = "PlayerId already taken by another player." });

        if (player.Version != request.Version)
        {
            return Conflict(new
            {
                error = "Concurrency conflict. Refresh the player and retry with the latest version.",
                currentVersion = player.Version
            });
        }

        player.PlayerId = request.PlayerId;
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
        player.SportMix30d = request.SportMix30d;
        player.BetTypeMix30d = request.BetTypeMix30d;
        player.StakeStdDev30d = request.StakeStdDev30d;
        player.TimeOfDayFitScore = request.TimeOfDayFitScore;
        player.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _db.SaveChangesAsync();
            return Ok(MapToResponse(player));
        }
        catch (DbUpdateConcurrencyException)
        {
            var latest = await _db.Players.AsNoTracking().FirstOrDefaultAsync(p => p.PlayerId == id);
            if (latest is null) return NotFound();

            return Conflict(new
            {
                error = "Concurrency conflict. The player was modified by another request.",
                current = MapToResponse(latest)
            });
        }
    }

    // DELETE api/players/{playerId}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(string id, [FromQuery] uint version)
    {
        if (!CanAccessPlayer(id))
        {
            return Forbid();
        }

        var player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerId == id);
        if (player is null) return NotFound();

        if (player.Version != version)
        {
            return Conflict(new
            {
                error = "Concurrency conflict. Refresh the player before deleting.",
                currentVersion = player.Version
            });
        }

        _db.Players.Remove(player);

        try
        {
            await _db.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict(new
            {
                error = "Concurrency conflict. The player was modified or removed by another request."
            });
        }
    }

    private static PlayerResponse MapToResponse(PlayerProfile p) => new()
    {
        Id = p.Id,
        Version = p.Version,
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
        SportMix30d = p.SportMix30d,
        BetTypeMix30d = p.BetTypeMix30d,
        StakeStdDev30d = p.StakeStdDev30d,
        TimeOfDayFitScore = p.TimeOfDayFitScore,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt
    };

    private bool IsAdmin()
    {
        return User.IsInRole("Admin");
    }

    private string? CurrentPlayerId()
    {
        return User.FindFirstValue("player_id");
    }

    private bool CanAccessPlayer(string playerId)
    {
        if (IsAdmin())
        {
            return true;
        }

        return string.Equals(CurrentPlayerId(), playerId, StringComparison.Ordinal);
    }
}
