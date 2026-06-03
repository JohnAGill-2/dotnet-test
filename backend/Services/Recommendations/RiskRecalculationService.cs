using dotnet_test.Data.Models;
using Microsoft.Extensions.Options;

namespace dotnet_test.Services.Recommendations;

public sealed class RiskRecalculationService : IRiskRecalculationService
{
    private static readonly Dictionary<string, int> RiskRank = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Low"] = 0,
        ["Medium"] = 1,
        ["High"] = 2,
        ["Very High"] = 3
    };

    private readonly RiskRecalculationOptions _options;

    public RiskRecalculationService(IOptions<RiskRecalculationOptions> options)
    {
        _options = options.Value;
    }

    public RiskRecalculationResult Recalculate(PlayerProfile player)
    {
        var previousRiskLevel = NormalizeRiskLevel(player.RiskLevel);
        var now = DateTime.UtcNow;

        var frequencyIncreased = IsFrequencyIncreaseDetected(player);
        var averageStakeIncreased = IsAverageStakeIncreaseDetected(player);
        var highVolatility = player.StakeStdDev30d >= _options.StakeVolatilityThreshold;
        var singleStakeSpike = player.MaxStake30d >= _options.SingleStakeSpikeThreshold;

        var triggers = new List<string>();
        if (frequencyIncreased)
        {
            triggers.Add($"Bet frequency increased by >= {_options.FrequencyIncreaseThresholdPercent:0}% versus previous 30-day window.");
        }

        if (averageStakeIncreased)
        {
            triggers.Add($"Average stake increased by >= {_options.AverageStakeIncreaseThresholdPercent:0}% versus previous 30-day window.");
        }

        if (highVolatility)
        {
            triggers.Add($"Stake volatility is high (std-dev >= {_options.StakeVolatilityThreshold:0.##}).");
        }

        if (singleStakeSpike)
        {
            triggers.Add($"Single stake spike detected (max stake >= {_options.SingleStakeSpikeThreshold:0.##}).");
        }

        var triggerCount = triggers.Count;
        var saferGamblingTriggered = triggerCount > 0;

        var proposedRisk = previousRiskLevel;
        if (triggerCount >= 2)
        {
            proposedRisk = "Very High";
        }
        else if (triggerCount == 1)
        {
            proposedRisk = "High";
        }

        if (RiskRank[previousRiskLevel] > RiskRank[proposedRisk])
        {
            proposedRisk = previousRiskLevel;
        }

        var changed = !string.Equals(previousRiskLevel, proposedRisk, StringComparison.OrdinalIgnoreCase);

        return new RiskRecalculationResult
        {
            PreviousRiskLevel = previousRiskLevel,
            NewRiskLevel = proposedRisk,
            RiskChanged = changed,
            SaferGamblingTriggered = saferGamblingTriggered,
            ChangeReason = saferGamblingTriggered ? string.Join(" ", triggers) : null,
            RecalculatedAtUtc = now
        };
    }

    private bool IsFrequencyIncreaseDetected(PlayerProfile player)
    {
        if (player.BetCountPrevious30d < _options.MinPreviousBetCountForTrend)
        {
            return false;
        }

        var threshold = player.BetCountPrevious30d * (1 + _options.FrequencyIncreaseThresholdPercent / 100d);
        return player.BetCount30d >= threshold;
    }

    private bool IsAverageStakeIncreaseDetected(PlayerProfile player)
    {
        if (player.AverageStakePrevious30d < _options.MinPreviousAverageStakeForTrend)
        {
            return false;
        }

        var threshold = player.AverageStakePrevious30d * (1 + (decimal)(_options.AverageStakeIncreaseThresholdPercent / 100d));
        return player.AverageStake >= threshold;
    }

    private static string NormalizeRiskLevel(string riskLevel)
    {
        if (RiskRank.ContainsKey(riskLevel))
        {
            return riskLevel;
        }

        return "Low";
    }
}
