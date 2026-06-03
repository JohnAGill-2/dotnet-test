namespace dotnet_test.Services.Recommendations;

public sealed class RiskRecalculationOptions
{
    public const string SectionName = "RiskRecalculation";

    public double FrequencyIncreaseThresholdPercent { get; set; } = 50;
    public double AverageStakeIncreaseThresholdPercent { get; set; } = 50;
    public decimal StakeVolatilityThreshold { get; set; } = 100m;
    public decimal SingleStakeSpikeThreshold { get; set; } = 200m;
    public int MinPreviousBetCountForTrend { get; set; } = 5;
    public decimal MinPreviousAverageStakeForTrend { get; set; } = 1m;
}
