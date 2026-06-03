using dotnet_test.Controllers.Contracts;
using dotnet_test.Data.Models;
using dotnet_test.Services.Claude;
using dotnet_test.Services.Recommendations;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace dotnet_test.Tests;

public class RecommendationContentGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_Uses_Claude_Response_When_Json_Is_Valid()
    {
        var generator = CreateGenerator(new StubClaudeClient(
            """
            {
              "headline": "Scotland are back in action",
              "message": "You usually follow Scotland markets. Here are today's available match markets.",
              "recommendationType": "Content",
              "reason": "Favourite team and betting preference matched.",
              "safeToShow": true
            }
            """));

        var result = await generator.GenerateAsync(BasePlayer(), AllowedResponse());

        Assert.Equal("Scotland are back in action", result.Headline);
        Assert.Equal("Content", result.RecommendationType);
        Assert.True(result.SafeToShow);
    }

    [Fact]
    public async Task GenerateAsync_Parses_Json_From_CodeFence_Response()
    {
        var generator = CreateGenerator(new StubClaudeClient(
            """
            Here is your result:
            ```json
            {
              "headline": "Weekly football picks",
              "message": "Single football bet options are currently strongest for your profile.",
              "recommendationType": "Content",
              "reason": "Top allowed option score and bet preference alignment.",
              "safeToShow": true
            }
            ```
            """));

        var result = await generator.GenerateAsync(BasePlayer(), AllowedResponse());

        Assert.Equal("Content", result.RecommendationType);
        Assert.True(result.SafeToShow);
        Assert.Equal("Weekly football picks", result.Headline);
    }

    [Fact]
    public async Task GenerateAsync_Returns_Fallback_When_Claude_Response_Is_Invalid()
    {
        var generator = CreateGenerator(new StubClaudeClient("{\"headline\":\"Missing fields\"}"));

        var result = await generator.GenerateAsync(BasePlayer(), AllowedResponse());

        Assert.Equal("Content", result.RecommendationType);
        Assert.True(result.SafeToShow);
        Assert.Contains("Scotland", result.Headline);
    }

    [Fact]
    public async Task GenerateAsync_Returns_Blocked_Fallback_Without_Calling_Claude()
    {
        var client = new StubClaudeClient("{}")
        {
            ThrowIfCalled = true
        };
        var generator = CreateGenerator(client);

        var response = new RecommendationResponse
        {
            PlayerId = "p-1",
            Blocked = true,
            BlockReason = "Player is self-excluded from recommendations."
        };

        var result = await generator.GenerateAsync(BasePlayer(), response);

        Assert.Equal("SaferGambling", result.RecommendationType);
        Assert.Equal("Recommendations are unavailable", result.Headline);
        Assert.True(result.SafeToShow);
    }

    [Fact]
    public async Task GenerateAsync_Returns_Fallback_When_Claude_Throws()
    {
        var generator = CreateGenerator(new StubClaudeClient("{}")
        {
            ExceptionToThrow = new HttpRequestException("boom")
        });

        var result = await generator.GenerateAsync(BasePlayer(), AllowedResponse());

        Assert.Equal("Content", result.RecommendationType);
        Assert.True(result.SafeToShow);
        Assert.Contains("Scotland", result.Message);
    }

    private static RecommendationContentGenerator CreateGenerator(IClaudeClient client)
    {
        var options = Options.Create(new ClaudeOptions
        {
            ApiKey = "test-key",
            BaseUrl = "https://api.anthropic.com",
            Model = "claude-sonnet-4-20250514",
            MaxTokens = 250,
            TimeoutSeconds = 10
        });

        return new RecommendationContentGenerator(client, options, NullLogger<RecommendationContentGenerator>.Instance);
    }

    private static PlayerProfile BasePlayer() => new()
    {
        PlayerId = "p-1",
        DaysSinceJoined = 200,
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

    private static RecommendationResponse AllowedResponse() => new()
    {
        PlayerId = "p-1",
        AllowedOptions =
        [
            new RecommendationOption
            {
                OptionType = "single_bet",
                Label = "Single Football bet",
                Score = 0.9
            }
        ]
    };

    private sealed class StubClaudeClient(string payload) : IClaudeClient
    {
        public bool ThrowIfCalled { get; set; }

        public Exception? ExceptionToThrow { get; set; }

        public Task<string> GenerateRecommendationContentAsync(string prompt, CancellationToken cancellationToken = default)
        {
            if (ThrowIfCalled)
            {
                throw new Xunit.Sdk.XunitException("Claude client should not have been called.");
            }

            if (ExceptionToThrow is not null)
            {
                throw ExceptionToThrow;
            }

            return Task.FromResult(payload);
        }
    }
}
