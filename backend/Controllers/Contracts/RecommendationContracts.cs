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

public class RecommendationContent
{
    public string Headline { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public bool SafeToShow { get; set; }
}

public class GuardrailDecision
{
    public bool IsBlocked { get; set; }
    public string? Reason { get; set; }
}

public class RiskRecalculationInfo
{
    public string PreviousRiskLevel { get; set; } = string.Empty;
    public string CurrentRiskLevel { get; set; } = string.Empty;
    public bool RiskChanged { get; set; }
    public bool SaferGamblingTriggered { get; set; }
    public string? ChangeReason { get; set; }
    public DateTime RecalculatedAtUtc { get; set; }
}

public class RecommendationResponse
{
    public string PlayerId { get; set; } = string.Empty;
    public bool Blocked { get; set; }
    public string? BlockReason { get; set; }
    public RecommendationContent? Content { get; set; }
    public List<RecommendationOption> AllowedOptions { get; set; } = new();
    public List<RecommendationOption> BlockedOptions { get; set; } = new();
    public List<RecommendationAuditItem> Audit { get; set; } = new();
    public List<string> Messages { get; set; } = new();
    public RiskRecalculationInfo? RiskRecalculation { get; set; }
}
