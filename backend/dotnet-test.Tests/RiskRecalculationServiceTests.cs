using dotnet_test.Data.Models;
using dotnet_test.Services.Recommendations;
using Microsoft.Extensions.Options;

namespace dotnet_test.Tests;

public class RiskRecalculationServiceTests
{
    private static readonly IOptions<RiskRecalculationOptions> DefaultOptions =
        Options.Create(new RiskRecalculationOptions
        {
            FrequencyIncreaseThresholdPercent = 50,
            AverageStakeIncreaseThresholdPercent = 50,
            StakeVolatilityThreshold = 100m,
            SingleStakeSpikeThreshold = 200m,
            MinPreviousBetCountForTrend = 5,
            MinPreviousAverageStakeForTrend = 1m
        });

    [Fact]
    public void Recalculate_Escalates_To_High_On_Frequency_Spike()
    {
        var service = new RiskRecalculationService(DefaultOptions);
        var player = BasePlayer();
        player.RiskLevel = "Medium";
        player.BetCountPrevious30d = 20;
        player.BetCount30d = 35; // +75%

        var result = service.Recalculate(player);

        Assert.True(result.SaferGamblingTriggered);
        Assert.True(result.RiskChanged);
        Assert.Equal("High", result.NewRiskLevel);
    }

    [Fact]
    public void Recalculate_Escalates_To_VeryHigh_When_Multiple_Triggers_Fire()
    {
        var service = new RiskRecalculationService(DefaultOptions);
        var player = BasePlayer();
        player.RiskLevel = "High";
        player.BetCountPrevious30d = 20;
        player.BetCount30d = 40;
        player.AverageStakePrevious30d = 80m;
        player.AverageStake = 140m;

        var result = service.Recalculate(player);

        Assert.True(result.SaferGamblingTriggered);
        Assert.Equal("Very High", result.NewRiskLevel);
    }

    [Fact]
    public void Recalculate_Does_Not_Change_Risk_When_No_Triggers_Fire()
    {
        var service = new RiskRecalculationService(DefaultOptions);
        var player = BasePlayer();
        player.RiskLevel = "Low";
        player.BetCountPrevious30d = 20;
        player.BetCount30d = 22;
        player.AverageStakePrevious30d = 20m;
        player.AverageStake = 21m;
        player.StakeStdDev30d = 20m;
        player.MaxStake30d = 30m;

        var result = service.Recalculate(player);

        Assert.False(result.SaferGamblingTriggered);
        Assert.False(result.RiskChanged);
        Assert.Equal("Low", result.NewRiskLevel);
    }

    [Fact]
    public void Recalculate_Triggers_SaferGambling_On_Volatility_Without_Trend_Baseline()
    {
        var service = new RiskRecalculationService(DefaultOptions);
        var player = BasePlayer();
        player.RiskLevel = "Low";
        player.BetCountPrevious30d = 0;
        player.AverageStakePrevious30d = 0m;
        player.StakeStdDev30d = 140m;

        var result = service.Recalculate(player);

        Assert.True(result.SaferGamblingTriggered);
        Assert.Equal("High", result.NewRiskLevel);
    }

    private static PlayerProfile BasePlayer() => new()
    {
        PlayerId = "p-risk",
        DaysSinceJoined = 120,
        MostBetSport = "Football",
        FavouriteTeam = "Scotland",
        MostBetType = "Single Bet",
        AverageStake = 20m,
        AverageStakePrevious30d = 20m,
        MaxStake30d = 30m,
        StakeStdDev30d = 20m,
        BetCount30d = 20,
        BetCountPrevious30d = 20,
        LastLoginDaysAgo = 2,
        RiskLevel = "Low",
        IsSelfExcluded = false,
        CoolingOffUntilUtc = null,
        JurisdictionCode = "UK",
        IsRecommendationRestricted = false,
        SportMix30d = 0.6,
        BetTypeMix30d = 0.6,
        TimeOfDayFitScore = 0.5
    };
}
