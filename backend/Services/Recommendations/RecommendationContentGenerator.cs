using System.Text.Json;
using dotnet_test.Controllers.Contracts;
using dotnet_test.Data.Models;
using dotnet_test.Services.Claude;
using Microsoft.Extensions.Options;

namespace dotnet_test.Services.Recommendations;

public class RecommendationContentGenerator : IRecommendationContentGenerator
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly HashSet<string> AllowedRecommendationTypes = new(StringComparer.Ordinal)
    {
        "Content",
        "SaferGambling"
    };

    private readonly IClaudeClient _claudeClient;
    private readonly ClaudeOptions _options;
    private readonly ILogger<RecommendationContentGenerator> _logger;

    public RecommendationContentGenerator(
        IClaudeClient claudeClient,
        IOptions<ClaudeOptions> options,
        ILogger<RecommendationContentGenerator> logger)
    {
        _claudeClient = claudeClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<RecommendationContent> GenerateAsync(
        PlayerProfile player,
        RecommendationResponse response,
        CancellationToken cancellationToken = default)
    {
        var fallback = BuildFallback(player, response);

        if (response.Blocked || response.AllowedOptions.Count == 0)
        {
            return fallback;
        }

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogInformation("Claude API key is not configured. Using deterministic recommendation content.");
            return fallback;
        }

        try
        {
            var prompt = BuildPrompt(player, response);
            var payload = await _claudeClient.GenerateRecommendationContentAsync(prompt, cancellationToken);
            var content = JsonSerializer.Deserialize<RecommendationContent>(payload, SerializerOptions);

            if (!IsValid(content))
            {
                _logger.LogWarning("Claude returned invalid recommendation content. Using fallback content.");
                return fallback;
            }

            return content!;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Claude content generation failed. Using fallback content.");
            return fallback;
        }
    }

    private static bool IsValid(RecommendationContent? content)
    {
        if (content is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(content.Headline) ||
            string.IsNullOrWhiteSpace(content.Message) ||
            string.IsNullOrWhiteSpace(content.Reason) ||
            !AllowedRecommendationTypes.Contains(content.RecommendationType))
        {
            return false;
        }

        return content.SafeToShow;
    }

    private static RecommendationContent BuildFallback(PlayerProfile player, RecommendationResponse response)
    {
        if (response.Blocked)
        {
            return new RecommendationContent
            {
                Headline = "Recommendations are unavailable",
                Message = response.BlockReason ?? "No safe recommendation options are available for this player.",
                RecommendationType = "SaferGambling",
                Reason = "Deterministic fallback generated from the blocked recommendation response.",
                SafeToShow = true
            };
        }

        var topOption = response.AllowedOptions
            .OrderByDescending(option => option.Score)
            .ThenBy(option => option.OptionType)
            .FirstOrDefault();

        if (topOption is null)
        {
            return new RecommendationContent
            {
                Headline = "No recommendation content is available",
                Message = "There are no safe recommendation options to show right now.",
                RecommendationType = "SaferGambling",
                Reason = "Deterministic fallback generated without any allowed recommendation options.",
                SafeToShow = true
            };
        }

        if (response.Messages.Contains("safer_gambling_resources", StringComparer.OrdinalIgnoreCase))
        {
            return new RecommendationContent
            {
                Headline = "Keep your play in control",
                Message = "Recent behavior signals suggest using lower-intensity options and safer gambling tools for now.",
                RecommendationType = "SaferGambling",
                Reason = "Deterministic fallback triggered by safer-gambling guardrail signals.",
                SafeToShow = true
            };
        }

        if (topOption.OptionType is "take_a_break" or "reduced_stake")
        {
            return new RecommendationContent
            {
                Headline = "Keep your play in control",
                Message = $"Your current recommendation mix favors lower-intensity options such as {topOption.Label.ToLowerInvariant()}.",
                RecommendationType = "SaferGambling",
                Reason = "Deterministic fallback matched the highest-ranked safer recommendation option.",
                SafeToShow = true
            };
        }

        return new RecommendationContent
        {
            Headline = $"{player.FavouriteTeam} markets are available",
            Message = $"You usually follow {player.FavouriteTeam} and prefer {player.MostBetType}. Here are today's available {player.MostBetSport} markets, including {topOption.Label}.",
            RecommendationType = "Content",
            Reason = "Deterministic fallback matched the player's favourite team, betting preference, and highest-ranked safe option.",
            SafeToShow = true
        };
    }

    private static string BuildPrompt(PlayerProfile player, RecommendationResponse response)
    {
        var promptData = new
        {
            player = new
            {
                player.PlayerId,
                player.MostBetSport,
                player.FavouriteTeam,
                player.MostBetType,
                player.RiskLevel,
                player.LastLoginDaysAgo
            },
            response = new
            {
                response.Blocked,
                response.BlockReason,
                response.Messages,
                AllowedOptions = response.AllowedOptions.Select(option => new
                {
                    option.OptionType,
                    option.Label,
                    option.Score
                })
            }
        };

                return """
Generate one user-facing recommendation content object.
Return JSON only.

Required JSON schema:
{
    "headline": "string",
    "message": "string",
    "recommendationType": "Content|SaferGambling",
    "reason": "string",
    "safeToShow": true
}

Rules:
- Use only the provided player and recommendation data.
- Do not invent unavailable options, events, or markets.
- Do not mention blocked options.
- recommendationType must be either Content or SaferGambling.
- safeToShow must be true.
- Keep the tone concise and factual.

Input:
""" + JsonSerializer.Serialize(promptData);
    }
}