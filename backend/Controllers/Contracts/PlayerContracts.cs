using System.ComponentModel.DataAnnotations;

namespace dotnet_test.Controllers.Contracts;

public class CreatePlayerRequest
{
    [Required]
    [MaxLength(50)]
    public string PlayerId { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int DaysSinceJoined { get; set; }

    [Required]
    [MaxLength(100)]
    public string MostBetSport { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FavouriteTeam { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string MostBetType { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "1000000")]
    public decimal AverageStake { get; set; }

    [Range(0, int.MaxValue)]
    public int LastLoginDaysAgo { get; set; }

    [Required]
    [MaxLength(20)]
    public string RiskLevel { get; set; } = "Low";

    public bool IsSelfExcluded { get; set; }

    public DateTime? CoolingOffUntilUtc { get; set; }

    [Required]
    [MaxLength(20)]
    public string JurisdictionCode { get; set; } = "UK";

    public bool IsRecommendationRestricted { get; set; }

    [Range(typeof(double), "0", "1")]
    public double SportMix30d { get; set; } = 0.5;

    [Range(typeof(double), "0", "1")]
    public double BetTypeMix30d { get; set; } = 0.5;

    [Range(typeof(decimal), "0", "1000000")]
    public decimal StakeStdDev30d { get; set; }

    [Range(typeof(double), "0", "1")]
    public double TimeOfDayFitScore { get; set; } = 0.5;
}

public class UpdatePlayerRequest
{
    [Required]
    [Range(1, uint.MaxValue)]
    public uint Version { get; set; }

    [Required]
    [MaxLength(50)]
    public string PlayerId { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int DaysSinceJoined { get; set; }

    [Required]
    [MaxLength(100)]
    public string MostBetSport { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FavouriteTeam { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string MostBetType { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "1000000")]
    public decimal AverageStake { get; set; }

    [Range(0, int.MaxValue)]
    public int LastLoginDaysAgo { get; set; }

    [Required]
    [MaxLength(20)]
    public string RiskLevel { get; set; } = "Low";

    public bool IsSelfExcluded { get; set; }

    public DateTime? CoolingOffUntilUtc { get; set; }

    [Required]
    [MaxLength(20)]
    public string JurisdictionCode { get; set; } = "UK";

    public bool IsRecommendationRestricted { get; set; }

    [Range(typeof(double), "0", "1")]
    public double SportMix30d { get; set; } = 0.5;

    [Range(typeof(double), "0", "1")]
    public double BetTypeMix30d { get; set; } = 0.5;

    [Range(typeof(decimal), "0", "1000000")]
    public decimal StakeStdDev30d { get; set; }

    [Range(typeof(double), "0", "1")]
    public double TimeOfDayFitScore { get; set; } = 0.5;
}

public class PlayerResponse
{
    public int Id { get; set; }
    public uint Version { get; set; }
    public string PlayerId { get; set; } = string.Empty;
    public int DaysSinceJoined { get; set; }
    public string MostBetSport { get; set; } = string.Empty;
    public string FavouriteTeam { get; set; } = string.Empty;
    public string MostBetType { get; set; } = string.Empty;
    public decimal AverageStake { get; set; }
    public int LastLoginDaysAgo { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public bool IsSelfExcluded { get; set; }
    public DateTime? CoolingOffUntilUtc { get; set; }
    public string JurisdictionCode { get; set; } = string.Empty;
    public bool IsRecommendationRestricted { get; set; }
    public double SportMix30d { get; set; }
    public double BetTypeMix30d { get; set; }
    public decimal StakeStdDev30d { get; set; }
    public double TimeOfDayFitScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
