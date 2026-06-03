using dotnet_test.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_test.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<PlayerProfile> Players => Set<PlayerProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PlayerProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PlayerId).IsUnique();
            entity.Property(e => e.PlayerId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.MostBetSport).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FavouriteTeam).IsRequired().HasMaxLength(100);
            entity.Property(e => e.MostBetType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.AverageStake).HasPrecision(10, 2);
            entity.Property(e => e.RiskLevel).IsRequired().HasMaxLength(20);
            entity.Property(e => e.JurisdictionCode).IsRequired().HasMaxLength(20);
        });
    }
}
