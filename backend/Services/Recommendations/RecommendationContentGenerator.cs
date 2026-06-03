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
            var primaryPrompt = BuildPrompt(player, response, strictRetry: false);
            var primaryPayload = await _claudeClient.GenerateRecommendationContentAsync(primaryPrompt, cancellationToken);

            if (TryParseAndValidate(primaryPayload, out var primaryContent))
            {
                return primaryContent!;
            }

            _logger.LogWarning("Claude primary response was invalid. Retrying with stricter prompt.");

            var retryPrompt = BuildPrompt(player, response, strictRetry: true);
            var retryPayload = await _claudeClient.GenerateRecommendationContentAsync(retryPrompt, cancellationToken);

            if (TryParseAndValidate(retryPayload, out var retryContent))
            {
                return retryContent!;
            }

            _logger.LogWarning("Claude retry response was invalid. Using fallback content.");
            return fallback;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidOperationException)
        {
            _logger.LogWarning(ex, "Claude content generation failed. Using fallback content.");
            return fallback;
        }
    }

    private static bool TryParseAndValidate(string payload, out RecommendationContent? content)
    {
        content = null;
        if (string.IsNullOrWhiteSpace(payload))
        {
            return false;
        }

        var json = ExtractJsonObject(payload) ?? payload;

        try
        {
            content = JsonSerializer.Deserialize<RecommendationContent>(json, SerializerOptions);
        }
        catch (JsonException)
        {
            return false;
        }

        return IsValid(content);
    }

    private static string? ExtractJsonObject(string payload)
    {
        var first = payload.IndexOf('{');
        var last = payload.LastIndexOf('}');
        if (first < 0 || last <= first)
        {
            return null;
        }

        return payload[first..(last + 1)];
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
                Reason = "Deterministic fallback generated from blocked recommendation response.",
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
                Reason = "Deterministic fallback triggered by safer-gambling signals.",
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
                Reason = "Deterministic fallback generated without allowed options.",
                SafeToShow = true
            };
        }

        return new RecommendationContent
        {
            Headline = $"{player.FavouriteTeam} markets are available",
            Message = $"You usually follow {player.FavouriteTeam} and prefer {player.MostBetType}. Available options include {topOption.Label}.",
            RecommendationType = "Content",
            Reason = "Deterministic fallback matched player preference and top safe option.",
            SafeToShow = true
        };
    }

    private static string BuildPrompt(PlayerProfile player, RecommendationResponse response, bool strictRetry)
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

        var rules = strictRetry
            ? "Return one JSON object only, with no markdown, no prose, and no code fences."
            : "Return JSON only.";

        return """
Generate one user-facing recommendation content object.
""" + rules +
"""

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
