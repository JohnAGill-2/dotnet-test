using System.ComponentModel.DataAnnotations;

namespace dotnet_test.Controllers.Contracts;

public class RecommendationQueryRequest
{
    [Range(1, 10)]
    public int MaxResults { get; set; } = 3;
}

public class RecommendationOption
{
    public string OptionType { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double Score { get; set; }
}

public class RecommendationAuditItem
{
    public string RuleId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double WeightImpact { get; set; }
}

public class RecommendationResponse
{
    public string PlayerId { get; set; } = string.Empty;
    public bool Blocked { get; set; }
    public string? BlockReason { get; set; }
    public List<RecommendationOption> AllowedOptions { get; set; } = new();
    public List<RecommendationOption> BlockedOptions { get; set; } = new();
    public List<RecommendationAuditItem> Audit { get; set; } = new();
    public List<string> Messages { get; set; } = new();
}
