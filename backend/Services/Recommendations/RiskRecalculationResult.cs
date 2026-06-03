namespace dotnet_test.Services.Recommendations;

public sealed class RiskRecalculationResult
{
    public string PreviousRiskLevel { get; init; } = string.Empty;
    public string NewRiskLevel { get; init; } = string.Empty;
    public bool RiskChanged { get; init; }
    public bool SaferGamblingTriggered { get; init; }
    public string? ChangeReason { get; init; }
    public DateTime RecalculatedAtUtc { get; init; }
}
