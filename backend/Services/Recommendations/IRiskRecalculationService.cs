using dotnet_test.Data.Models;

namespace dotnet_test.Services.Recommendations;

public interface IRiskRecalculationService
{
    RiskRecalculationResult Recalculate(PlayerProfile player);
}
