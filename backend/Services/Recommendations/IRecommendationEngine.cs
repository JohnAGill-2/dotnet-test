using dotnet_test.Controllers.Contracts;
using dotnet_test.Data.Models;

namespace dotnet_test.Services.Recommendations;

public interface IRecommendationEngine
{
    RecommendationResponse Evaluate(PlayerProfile player, int maxResults);
}
