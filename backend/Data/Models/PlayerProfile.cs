using System.ComponentModel.DataAnnotations;

namespace dotnet_test.Data.Models;

public class PlayerProfile
{
    public int Id { get; set; }

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
