namespace dotnet_test.Services.Claude;

public interface IClaudeClient
{
    Task<string> GenerateRecommendationContentAsync(string prompt, CancellationToken cancellationToken = default);
}
