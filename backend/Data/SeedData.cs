using dotnet_test.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_test.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        if (await db.Players.AnyAsync()) return;

        db.Players.AddRange(
            new PlayerProfile
            {
                PlayerId = "12345",
                DaysSinceJoined = 180,
                MostBetSport = "Football",
                FavouriteTeam = "Scotland",
                MostBetType = "Bet Builder",
                AverageStake = 12.50m,
                LastLoginDaysAgo = 6,
                RiskLevel = "Low",
                IsSelfExcluded = false,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                CreatedAt = DateTime.UtcNow
            },
            new PlayerProfile
            {
                PlayerId = "12346",
                DaysSinceJoined = 60,
                MostBetSport = "Tennis",
                FavouriteTeam = "Andy Murray",
                MostBetType = "Accumulator",
                AverageStake = 8.75m,
                LastLoginDaysAgo = 2,
                RiskLevel = "Medium",
                IsSelfExcluded = false,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                CreatedAt = DateTime.UtcNow
            },
            new PlayerProfile
            {
                PlayerId = "12347",
                DaysSinceJoined = 365,
                MostBetSport = "Horse Racing",
                FavouriteTeam = "N/A",
                MostBetType = "Each Way",
                AverageStake = 25.00m,
                LastLoginDaysAgo = 1,
                RiskLevel = "High",
                IsSelfExcluded = false,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = true,
                CreatedAt = DateTime.UtcNow
            },
            new PlayerProfile
            {
                PlayerId = "12348",
                DaysSinceJoined = 14,
                MostBetSport = "Basketball",
                FavouriteTeam = "LA Lakers",
                MostBetType = "Single",
                AverageStake = 5.00m,
                LastLoginDaysAgo = 0,
                RiskLevel = "Low",
                IsSelfExcluded = false,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                CreatedAt = DateTime.UtcNow
            },
            new PlayerProfile
            {
                PlayerId = "12349",
                DaysSinceJoined = 730,
                MostBetSport = "Football",
                FavouriteTeam = "Manchester United",
                MostBetType = "In-Play",
                AverageStake = 50.00m,
                LastLoginDaysAgo = 30,
                RiskLevel = "High",
                IsSelfExcluded = true,
                CoolingOffUntilUtc = DateTime.UtcNow.AddDays(14),
                JurisdictionCode = "UK",
                IsRecommendationRestricted = true,
                CreatedAt = DateTime.UtcNow
            },
            new PlayerProfile
            {
                PlayerId = "12354",
                DaysSinceJoined = 90,
                MostBetSport = "Cricket",
                FavouriteTeam = "England",
                MostBetType = "Accumulator",
                AverageStake = 15.00m,
                LastLoginDaysAgo = 3,
                RiskLevel = "Medium",
                IsSelfExcluded = false,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                CreatedAt = DateTime.UtcNow
            }
        );

        await db.SaveChangesAsync();
    }
}
