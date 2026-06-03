using dotnet_test.Controllers.Contracts;
using dotnet_test.Data;
using dotnet_test.Services.Recommendations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_test.Controllers;

[ApiController]
[Route("api/players/{id}/recommendations")]
public class RecommendationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IRecommendationEngine _engine;

    public RecommendationsController(AppDbContext db, IRecommendationEngine engine)
    {
        _db = db;
        _engine = engine;
    }

    [HttpPost]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RecommendationResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Recommend(string id, [FromBody] RecommendationQueryRequest? request, CancellationToken cancellationToken)
    {
        var player = await _db.Players.FirstOrDefaultAsync(p => p.PlayerId == id, cancellationToken);
        if (player is null) return NotFound();

        var result = _engine.Evaluate(player, request?.MaxResults ?? 3);
        if (result.Blocked) return StatusCode(StatusCodes.Status403Forbidden, result);

        return Ok(result);
    }
}
