using dotnet_test.Controllers.Contracts;
using dotnet_test.Data;
using dotnet_test.Services.Recommendations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace dotnet_test.Controllers;

[ApiController]
[Authorize]
[Route("api/players/{id}/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IRecommendationEngine _engine;
    private readonly IRecommendationContentGenerator _contentGenerator;
    private readonly IRiskRecalculationService _riskRecalculationService;

    public RecommendationsController(
        AppDbContext db,
        IRecommendationEngine engine,
        IRecommendationContentGenerator contentGenerator,
        IRiskRecalculationService riskRecalculationService)
    {
        _db = db;
        _engine = engine;
        _contentGenerator = contentGenerator;
        _riskRecalculationService = riskRecalculationService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Recommend(string id, [FromBody] RecommendationQueryRequest? request, CancellationToken cancellationToken)
    {
        if (!CanAccessPlayer(id))
        {
            return Forbid();
        }

        var player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerId == id, cancellationToken);
        if (player is null) return NotFound();

        var riskResult = _riskRecalculationService.Recalculate(player);
        if (riskResult.RiskChanged)
        {
            player.RiskLevel = riskResult.NewRiskLevel;
            player.RiskChangeReason = riskResult.ChangeReason;
            player.LastRiskRecalculatedUtc = riskResult.RecalculatedAtUtc;
            await _db.SaveChangesAsync(cancellationToken);
        }

        var result = _engine.Evaluate(player, request?.MaxResults ?? 3);
        if (riskResult.SaferGamblingTriggered && !result.Messages.Contains("safer_gambling_resources"))
        {
            result.Messages.Add("safer_gambling_resources");
        }

        result.RiskRecalculation = new RiskRecalculationInfo
        {
            PreviousRiskLevel = riskResult.PreviousRiskLevel,
            CurrentRiskLevel = riskResult.NewRiskLevel,
            RiskChanged = riskResult.RiskChanged,
            SaferGamblingTriggered = riskResult.SaferGamblingTriggered,
            ChangeReason = riskResult.ChangeReason,
            RecalculatedAtUtc = riskResult.RecalculatedAtUtc
        };

        result.Content = await _contentGenerator.GenerateAsync(player, result, cancellationToken);
        if (result.Blocked) return StatusCode(StatusCodes.Status403Forbidden, result);

        return Ok(result);
    }

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
