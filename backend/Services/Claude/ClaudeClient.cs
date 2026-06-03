using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace dotnet_test.Services.Claude;

public class ClaudeClient : IClaudeClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;

    public ClaudeClient(HttpClient httpClient, IOptions<ClaudeOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> GenerateRecommendationContentAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("Claude API key is not configured.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, "v1/messages")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new ClaudeRequest(_options.Model, _options.MaxTokens, prompt)),
                Encoding.UTF8,
                "application/json")
        };

        request.Headers.Add("x-api-key", _options.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var payload = await JsonSerializer.DeserializeAsync<ClaudeResponse>(responseStream, SerializerOptions, cancellationToken);
        var text = payload?.Content
            .FirstOrDefault(item => string.Equals(item.Type, "text", StringComparison.OrdinalIgnoreCase))?
            .Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Claude returned an empty content payload.");
        }

        return text;
    }

    private sealed record ClaudeRequest(string Model, int MaxTokens, string Prompt)
    {
        public string System { get; } = "You generate structured JSON recommendation content for a sportsbook API.";
        public object[] Messages { get; } =
        [
            new
            {
                role = "user",
                content = Prompt
            }
        ];
    }

    private sealed class ClaudeResponse
    {
        public List<ClaudeContentBlock> Content { get; set; } = [];
    }

    private sealed class ClaudeContentBlock
    {
        public string Type { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}