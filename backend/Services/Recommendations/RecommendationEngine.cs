using dotnet_test.Controllers.Contracts;
using dotnet_test.Data.Models;

namespace dotnet_test.Services.Recommendations;

public class RecommendationEngine : IRecommendationEngine
{
    private static readonly string[] HighIntensity = ["bet_builder", "accumulator"];

    public RecommendationResponse Evaluate(PlayerProfile player, int maxResults)
    {
        var response = new RecommendationResponse
        {
            PlayerId = player.PlayerId
        };

        var options = BuildCandidateOptions(player);

        // Hard guardrails: block regardless of score if player is restricted.
        var hardBlocked = IsHardBlocked(player, response);
        var allowed = ApplyOptionGuardrails(player, options, response);

        if (hardBlocked || allowed.Count == 0)
        {
            response.Blocked = true;
            if (string.IsNullOrWhiteSpace(response.BlockReason))
            {
                response.BlockReason = "No safe recommendation options are available for this player.";
            }
            return response;
        }

        response.AllowedOptions = allowed
            .OrderByDescending(o => o.Score)
            .ThenBy(o => o.OptionType)
            .Take(maxResults)
            .ToList();

        AppendSafetyMessage(player, response);
        return response;
    }

    private static bool IsHardBlocked(PlayerProfile player, RecommendationResponse response)
    {
        if (player.IsSelfExcluded)
        {
            response.Blocked = true;
            response.BlockReason = "Player is self-excluded from recommendations.";
            response.Audit.Add(new RecommendationAuditItem
            {
                RuleId = "self_excluded_hard_block",
                Description = "Self-excluded players are always blocked.",
                WeightImpact = 0
            });
            return true;
        }

        if (player.CoolingOffUntilUtc.HasValue && player.CoolingOffUntilUtc.Value > DateTime.UtcNow)
        {
            response.Blocked = true;
            response.BlockReason = "Player is currently in a cooling-off period.";
            response.Audit.Add(new RecommendationAuditItem
            {
                RuleId = "cooling_off_hard_block",
                Description = "Cooling-off period prevents recommendation output.",
                WeightImpact = 0
            });
            return true;
        }

        if (player.IsRecommendationRestricted)
        {
            response.Blocked = true;
            response.BlockReason = "Recommendations are restricted for this player jurisdiction/profile.";
            response.Audit.Add(new RecommendationAuditItem
            {
                RuleId = "jurisdiction_restriction_hard_block",
                Description = "Compliance restriction prevents recommendations.",
                WeightImpact = 0
            });
            return true;
        }

        return false;
    }

    private static List<RecommendationOption> BuildCandidateOptions(PlayerProfile player)
    {
        var options = new List<RecommendationOption>
        {
            new() { OptionType = "single_bet", Label = $"Single {player.MostBetSport} bet", Score = 0.30 },
            new() { OptionType = "bet_builder", Label = "Bet Builder", Score = 0.20 },
            new() { OptionType = "accumulator", Label = "Accumulator", Score = 0.20 },
            new() { OptionType = "reduced_stake", Label = "Reduced stake option", Score = 0.15 },
            new() { OptionType = "take_a_break", Label = "Take-a-break recommendation", Score = 0.15 }
        };

        var sportAffinity = Math.Clamp(player.SportMix30d, 0, 1);
        var betTypeAffinity = Math.Clamp(player.BetTypeMix30d, 0, 1);
        var recencyFit = player.LastLoginDaysAgo > 14 ? 1.0 : 0.2;
        var timeFit = Math.Clamp(player.TimeOfDayFitScore, 0, 1);

        foreach (var option in options)
        {
            var score = option.Score;

            score += 0.40 * sportAffinity;
            score += 0.30 * betTypeAffinity;
            score += 0.20 * recencyFit;
            score += 0.10 * timeFit;

            if (player.AverageStake >= 250m)
            {
                score -= 0.15;
            }

            if (player.StakeStdDev30d >= 100m)
            {
                score -= 0.10;
            }

            if (player.MostBetType.Equals("Bet Builder", StringComparison.OrdinalIgnoreCase) && option.OptionType == "bet_builder")
            {
                score += 0.10;
            }

            if (player.MostBetType.Equals("Accumulator", StringComparison.OrdinalIgnoreCase) && option.OptionType == "accumulator")
            {
                score += 0.10;
            }

            option.Score = Math.Round(Math.Clamp(score, 0, 1), 3);
        }

        return options;
    }

    private static List<RecommendationOption> ApplyOptionGuardrails(PlayerProfile player, List<RecommendationOption> options, RecommendationResponse response)
    {
        var allowed = new List<RecommendationOption>();

        foreach (var option in options)
        {
            var blocked = false;

            if (IsHighRisk(player) && HighIntensity.Contains(option.OptionType))
            {
                blocked = true;
                response.BlockedOptions.Add(option);
                response.Audit.Add(new RecommendationAuditItem
                {
                    RuleId = option.OptionType == "bet_builder"
                        ? "high_risk_blocks_bet_builder"
                        : "high_risk_blocks_accumulator",
                    Description = "High-risk players cannot receive high-intensity recommendation options.",
                    WeightImpact = 0
                });
            }

            if (!blocked)
            {
                allowed.Add(option);
            }
        }

        if (player.LastLoginDaysAgo > 14)
        {
            response.Audit.Add(new RecommendationAuditItem
            {
                RuleId = "recency_reactivation_bonus",
                Description = "Recency threshold exceeded; reactivation-friendly options are prioritized.",
                WeightImpact = 0.20
            });
        }

        if (player.AverageStake >= 250m)
        {
            response.Audit.Add(new RecommendationAuditItem
            {
                RuleId = "high_average_stake_caution",
                Description = "High average stake triggers caution penalty across recommendations.",
                WeightImpact = -0.15
            });
        }

        if (player.StakeStdDev30d >= 100m)
        {
            response.Audit.Add(new RecommendationAuditItem
            {
                RuleId = "stake_volatility_caution",
                Description = "High stake volatility triggers safety penalty.",
                WeightImpact = -0.10
            });
        }

        return allowed;
    }

    private static bool IsHighRisk(PlayerProfile player)
    {
        return player.RiskLevel.Equals("High", StringComparison.OrdinalIgnoreCase)
            || player.RiskLevel.Equals("Very High", StringComparison.OrdinalIgnoreCase);
    }

    private static void AppendSafetyMessage(PlayerProfile player, RecommendationResponse response)
    {
        if (IsHighRisk(player)
            || player.AverageStake >= 250m
            || player.StakeStdDev30d >= 100m)
        {
            if (!response.Messages.Contains("safer_gambling_resources"))
            {
                response.Messages.Add("safer_gambling_resources");
            }
        }
    }
}
