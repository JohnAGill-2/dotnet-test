using dotnet_test.Data.Models;
using dotnet_test.Services.Recommendations;

namespace dotnet_test.Tests;

public class RecommendationEngineTests
{
    private readonly RecommendationEngine _engine = new();

    [Fact]
    public void SelfExcluded_Player_Is_Hard_Blocked()
    {
        var player = BasePlayer();
        player.IsSelfExcluded = true;

        var result = _engine.Evaluate(player, 5);

        Assert.True(result.Blocked);
        Assert.Equal("Player is self-excluded from recommendations.", result.BlockReason);
        Assert.Contains(result.Audit, a => a.RuleId == "self_excluded_hard_block");
    }

    [Fact]
    public void HighRisk_Blocks_HighIntensity_Options()
    {
        var player = BasePlayer();
        player.RiskLevel = "High";

        var result = _engine.Evaluate(player, 10);

        Assert.DoesNotContain(result.AllowedOptions, o => o.OptionType == "bet_builder");
        Assert.DoesNotContain(result.AllowedOptions, o => o.OptionType == "accumulator");
        Assert.Contains(result.BlockedOptions, o => o.OptionType == "bet_builder");
        Assert.Contains(result.BlockedOptions, o => o.OptionType == "accumulator");
        Assert.Contains("safer_gambling_resources", result.Messages);
    }

    [Fact]
    public void Recency_Over14_Adds_Reactivation_Audit()
    {
        var player = BasePlayer();
        player.LastLoginDaysAgo = 15;

        var result = _engine.Evaluate(player, 10);

        Assert.Contains(result.Audit, a => a.RuleId == "recency_reactivation_bonus");
        Assert.Contains(result.AllowedOptions, o => o.OptionType == "welcome_back");
    }

    [Fact]
    public void MaxResults_Limits_Returned_Options()
    {
        var player = BasePlayer();

        var result = _engine.Evaluate(player, 2);

        Assert.False(result.Blocked);
        Assert.Equal(2, result.AllowedOptions.Count);
    }

    private static PlayerProfile BasePlayer() => new()
    {
        PlayerId = "p-1",
        DaysSinceJoined = 120,
        MostBetSport = "Football",
        FavouriteTeam = "Scotland",
        MostBetType = "Bet Builder",
        AverageStake = 30m,
        LastLoginDaysAgo = 2,
        RiskLevel = "Low",
        IsSelfExcluded = false,
        CoolingOffUntilUtc = null,
        JurisdictionCode = "UK",
        IsRecommendationRestricted = false
    };
}
