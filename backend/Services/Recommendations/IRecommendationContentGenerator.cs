using dotnet_test.Controllers.Contracts;
using dotnet_test.Data.Models;

namespace dotnet_test.Services.Recommendations;

public interface IRecommendationContentGenerator
{
    Task<RecommendationContent> GenerateAsync(PlayerProfile player, RecommendationResponse response, CancellationToken cancellationToken = default);
}
