using dotnet_test.Data.Models;
using dotnet_test.Services.Recommendations;

namespace dotnet_test.Tests;

public class RecommendationEngineTests
{
    private readonly RecommendationEngine _engine = new();

    [Fact]
    public void HighRisk_Blocks_BetBuilder_And_Accumulator()
    {
        var player = BasePlayer();
        player.RiskLevel = "High";

        var result = _engine.Evaluate(player, 10);

        Assert.DoesNotContain(result.AllowedOptions, o => o.OptionType == "bet_builder");
        Assert.DoesNotContain(result.AllowedOptions, o => o.OptionType == "accumulator");
        Assert.Contains(result.BlockedOptions, o => o.OptionType == "bet_builder");
        Assert.Contains(result.BlockedOptions, o => o.OptionType == "accumulator");
        Assert.Contains(result.Audit, a => a.RuleId == "high_risk_blocks_bet_builder");
        Assert.Contains(result.Audit, a => a.RuleId == "high_risk_blocks_accumulator");
    }

    [Fact]
    public void VeryHighRisk_Blocks_BetBuilder_And_Accumulator()
    {
        var player = BasePlayer();
        player.RiskLevel = "Very High";

        var result = _engine.Evaluate(player, 10);

        Assert.DoesNotContain(result.AllowedOptions, o => o.OptionType == "bet_builder");
        Assert.DoesNotContain(result.AllowedOptions, o => o.OptionType == "accumulator");
        Assert.Contains(result.BlockedOptions, o => o.OptionType == "bet_builder");
        Assert.Contains(result.BlockedOptions, o => o.OptionType == "accumulator");
    }

    [Fact]
    public void Recency_Over14_Emits_Reactivation_Audit_Rule()
    {
        var player = BasePlayer();
        player.LastLoginDaysAgo = 15;

        var result = _engine.Evaluate(player, 5);

        Assert.Contains(result.Audit, a => a.RuleId == "recency_reactivation_bonus");
    }

    [Fact]
    public void AverageStake_Above250_Triggers_Caution_Rule()
    {
        var player = BasePlayer();
        player.AverageStake = 250m;

        var result = _engine.Evaluate(player, 5);

        Assert.Contains(result.Audit, a => a.RuleId == "high_average_stake_caution");
    }

    [Fact]
    public void Blocked_Response_Includes_Audit_Details()
    {
        var player = BasePlayer();
        player.IsSelfExcluded = true;

        var result = _engine.Evaluate(player, 5);

        Assert.True(result.Blocked);
        Assert.NotEmpty(result.Audit);
        Assert.Contains(result.Audit, a => a.RuleId == "self_excluded_hard_block");
    }

    private static PlayerProfile BasePlayer() => new()
    {
        PlayerId = "p-1",
        DaysSinceJoined = 200,
        MostBetSport = "Football",
        FavouriteTeam = "Scotland",
        MostBetType = "Bet Builder",
        AverageStake = 30m,
        StakeStdDev30d = 12m,
        LastLoginDaysAgo = 2,
        RiskLevel = "Low",
        IsSelfExcluded = false,
        CoolingOffUntilUtc = null,
        JurisdictionCode = "UK",
        IsRecommendationRestricted = false,
        SportMix30d = 0.8,
        BetTypeMix30d = 0.75,
        TimeOfDayFitScore = 0.6
    };
}
