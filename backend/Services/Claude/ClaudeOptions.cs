using System.ComponentModel.DataAnnotations;

namespace dotnet_test.Services.Claude;

public class ClaudeOptions
{
    public const string SectionName = "Claude";

    [Required]
    public string BaseUrl { get; set; } = "https://api.anthropic.com";

    [Required]
    public string Model { get; set; } = "claude-sonnet-4-20250514";

    [Range(1, 4096)]
    public int MaxTokens { get; set; } = 250;

    [Range(1, 120)]
    public int TimeoutSeconds { get; set; } = 10;

    public string ApiKey { get; set; } = string.Empty;
}