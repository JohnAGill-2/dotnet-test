using dotnet_test.Data;
using dotnet_test.Data.Models;
using dotnet_test.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace dotnet_test.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new AppUser
                {
                    UserName = "admin",
                    PasswordHash = PasswordHasher.HashPassword("admin123!"),
                    Role = "Admin",
                    PlayerId = null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new AppUser
                {
                    UserName = "user12345",
                    PasswordHash = PasswordHasher.HashPassword("user12345!"),
                    Role = "User",
                    PlayerId = "12345",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new AppUser
                {
                    UserName = "user12354",
                    PasswordHash = PasswordHasher.HashPassword("user12354!"),
                    Role = "User",
                    PlayerId = "12354",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );

            await db.SaveChangesAsync();
        }

        var existingPlayerIds = await db.Players
            .Select(p => p.PlayerId)
            .ToHashSetAsync();

        var now = DateTime.UtcNow;
        var seededPlayers = new List<PlayerProfile>
        {
            // Original 3 seeded players
            new PlayerProfile
            {
                PlayerId = "12345",
                DaysSinceJoined = 180,
                MostBetSport = "Football",
                FavouriteTeam = "Scotland",
                MostBetType = "Bet Builder",
                AverageStake = 12.50m,
                AverageStakePrevious30d = 12.00m,
                MaxStake30d = 24.00m,
                StakeStdDev30d = 6.25m,
                BetCount30d = 28,
                BetCountPrevious30d = 26,
                LastLoginDaysAgo = 6,
                RiskLevel = "Low",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.82,
                BetTypeMix30d = 0.79,
                TimeOfDayFitScore = 0.68,
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
                AverageStakePrevious30d = 8.20m,
                MaxStake30d = 18.00m,
                StakeStdDev30d = 12.00m,
                BetCount30d = 18,
                BetCountPrevious30d = 16,
                LastLoginDaysAgo = 2,
                RiskLevel = "Medium",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.73,
                BetTypeMix30d = 0.77,
                TimeOfDayFitScore = 0.61,
                CreatedAt = DateTime.UtcNow
            },
            new PlayerProfile
            {
                PlayerId = "12347",
                DaysSinceJoined = 400,
                MostBetSport = "Horse Racing",
                FavouriteTeam = "N/A",
                MostBetType = "Win",
                AverageStake = 25.00m,
                AverageStakePrevious30d = 22.50m,
                MaxStake30d = 74.00m,
                StakeStdDev30d = 132.00m,
                BetCount30d = 38,
                BetCountPrevious30d = 30,
                LastLoginDaysAgo = 1,
                RiskLevel = "High",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.64,
                BetTypeMix30d = 0.55,
                TimeOfDayFitScore = 0.52,
                CreatedAt = DateTime.UtcNow
            },
            // 10 additional varied players
            // Hard block: Self-excluded
            new PlayerProfile
            {
                PlayerId = "12348",
                DaysSinceJoined = 250,
                MostBetSport = "Cricket",
                FavouriteTeam = "England",
                MostBetType = "Single Bet",
                AverageStake = 15.00m,
                StakeStdDev30d = 7.50m,
                LastLoginDaysAgo = 10,
                RiskLevel = "Low",
                IsSelfExcluded = true,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.71,
                BetTypeMix30d = 0.68,
                TimeOfDayFitScore = 0.74,
                CreatedAt = DateTime.UtcNow
            },
            // Hard block: Active cooling-off period (7 days remaining)
            new PlayerProfile
            {
                PlayerId = "12349",
                DaysSinceJoined = 120,
                MostBetSport = "Rugby",
                FavouriteTeam = "Wales",
                MostBetType = "Accumulator",
                AverageStake = 20.00m,
                StakeStdDev30d = 45.00m,
                LastLoginDaysAgo = 5,
                RiskLevel = "Medium",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = now.AddDays(7),
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.66,
                BetTypeMix30d = 0.72,
                TimeOfDayFitScore = 0.58,
                CreatedAt = DateTime.UtcNow
            },
            // Hard block: Jurisdiction-restricted
            new PlayerProfile
            {
                PlayerId = "12350",
                DaysSinceJoined = 300,
                MostBetSport = "Basketball",
                FavouriteTeam = "Chicago Bulls",
                MostBetType = "Bet Builder",
                AverageStake = 18.00m,
                StakeStdDev30d = 8.00m,
                LastLoginDaysAgo = 3,
                RiskLevel = "Low",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "JP",
                IsRecommendationRestricted = true,
                SportMix30d = 0.75,
                BetTypeMix30d = 0.80,
                TimeOfDayFitScore = 0.69,
                CreatedAt = DateTime.UtcNow
            },
            // Brand new player (low recency)
            new PlayerProfile
            {
                PlayerId = "12351",
                DaysSinceJoined = 3,
                MostBetSport = "Football",
                FavouriteTeam = "Manchester United",
                MostBetType = "Single Bet",
                AverageStake = 5.00m,
                StakeStdDev30d = 2.50m,
                LastLoginDaysAgo = 1,
                RiskLevel = "Low",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.45,
                BetTypeMix30d = 0.40,
                TimeOfDayFitScore = 0.55,
                CreatedAt = DateTime.UtcNow
            },
            // Very active, daily player
            new PlayerProfile
            {
                PlayerId = "12352",
                DaysSinceJoined = 800,
                MostBetSport = "Football",
                FavouriteTeam = "Liverpool",
                MostBetType = "Bet Builder",
                AverageStake = 22.00m,
                StakeStdDev30d = 9.00m,
                LastLoginDaysAgo = 0,
                RiskLevel = "Medium",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.88,
                BetTypeMix30d = 0.85,
                TimeOfDayFitScore = 0.92,
                CreatedAt = DateTime.UtcNow
            },
            // Dormant player (no login in 60+ days)
            new PlayerProfile
            {
                PlayerId = "12353",
                DaysSinceJoined = 500,
                MostBetSport = "Tennis",
                FavouriteTeam = "Novak Djokovic",
                MostBetType = "Win",
                AverageStake = 30.00m,
                StakeStdDev30d = 85.00m,
                LastLoginDaysAgo = 65,
                RiskLevel = "High",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.51,
                BetTypeMix30d = 0.48,
                TimeOfDayFitScore = 0.35,
                CreatedAt = DateTime.UtcNow
            },
            // Very high stake, very risky player
            new PlayerProfile
            {
                PlayerId = "12354",
                DaysSinceJoined = 350,
                MostBetSport = "Horse Racing",
                FavouriteTeam = "N/A",
                MostBetType = "Accumulator",
                AverageStake = 250.00m,
                AverageStakePrevious30d = 120.00m,
                MaxStake30d = 450.00m,
                StakeStdDev30d = 500.00m,
                BetCount30d = 42,
                BetCountPrevious30d = 21,
                LastLoginDaysAgo = 2,
                RiskLevel = "Very High",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.42,
                BetTypeMix30d = 0.38,
                TimeOfDayFitScore = 0.44,
                CreatedAt = DateTime.UtcNow
            },
            // Conservative player (low stakes, very consistent)
            new PlayerProfile
            {
                PlayerId = "12355",
                DaysSinceJoined = 600,
                MostBetSport = "Golf",
                FavouriteTeam = "Rory McIlroy",
                MostBetType = "Single Bet",
                AverageStake = 2.50m,
                AverageStakePrevious30d = 2.60m,
                MaxStake30d = 5.00m,
                StakeStdDev30d = 0.75m,
                BetCount30d = 8,
                BetCountPrevious30d = 9,
                LastLoginDaysAgo = 12,
                RiskLevel = "Low",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.94,
                BetTypeMix30d = 0.91,
                TimeOfDayFitScore = 0.87,
                CreatedAt = DateTime.UtcNow
            },
            // Mixed preferences (low affinity)
            new PlayerProfile
            {
                PlayerId = "12356",
                DaysSinceJoined = 200,
                MostBetSport = "Darts",
                FavouriteTeam = "Luke Humphries",
                MostBetType = "Both Ways",
                AverageStake = 7.50m,
                StakeStdDev30d = 15.00m,
                LastLoginDaysAgo = 20,
                RiskLevel = "Medium",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.35,
                BetTypeMix30d = 0.32,
                TimeOfDayFitScore = 0.41,
                CreatedAt = DateTime.UtcNow
            },
            // Perfect fit: high affinity, consistent, medium risk
            new PlayerProfile
            {
                PlayerId = "12357",
                DaysSinceJoined = 450,
                MostBetSport = "Ice Hockey",
                FavouriteTeam = "Vegas Golden Knights",
                MostBetType = "Bet Builder",
                AverageStake = 16.00m,
                StakeStdDev30d = 5.00m,
                LastLoginDaysAgo = 4,
                RiskLevel = "Medium",
                IsSelfExcluded = false,
                CoolingOffUntilUtc = null,
                JurisdictionCode = "UK",
                IsRecommendationRestricted = false,
                SportMix30d = 0.86,
                BetTypeMix30d = 0.84,
                TimeOfDayFitScore = 0.78,
                CreatedAt = DateTime.UtcNow
            }
        };

        var missingPlayers = seededPlayers
            .Where(player => !existingPlayerIds.Contains(player.PlayerId))
            .ToList();

        if (missingPlayers.Count > 0)
        {
            db.Players.AddRange(missingPlayers);
            await db.SaveChangesAsync();
        }
    }
}
