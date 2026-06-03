using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_test.Migrations
{
    /// <inheritdoc />
    public partial class BackfillRecommendationSignals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Backfill existing rows created before Week 2 fields existed.
            migrationBuilder.Sql(@"
UPDATE ""Players""
SET
    ""IsSelfExcluded"" = FALSE,
    ""CoolingOffUntilUtc"" = NULL,
    ""JurisdictionCode"" = 'UK',
    ""IsRecommendationRestricted"" = FALSE,
    ""SportMix30d"" = 0.82,
    ""BetTypeMix30d"" = 0.79,
    ""StakeStdDev30d"" = 6.25,
    ""TimeOfDayFitScore"" = 0.68
WHERE ""PlayerId"" = '12345';

UPDATE ""Players""
SET
    ""IsSelfExcluded"" = FALSE,
    ""CoolingOffUntilUtc"" = NULL,
    ""JurisdictionCode"" = 'UK',
    ""IsRecommendationRestricted"" = FALSE,
    ""SportMix30d"" = 0.73,
    ""BetTypeMix30d"" = 0.77,
    ""StakeStdDev30d"" = 12.00,
    ""TimeOfDayFitScore"" = 0.61
WHERE ""PlayerId"" = '12346';

UPDATE ""Players""
SET
    ""IsSelfExcluded"" = FALSE,
    ""CoolingOffUntilUtc"" = NULL,
    ""JurisdictionCode"" = 'UK',
    ""IsRecommendationRestricted"" = FALSE,
    ""SportMix30d"" = 0.64,
    ""BetTypeMix30d"" = 0.55,
    ""StakeStdDev30d"" = 132.00,
    ""TimeOfDayFitScore"" = 0.52
WHERE ""PlayerId"" = '12347';

UPDATE ""Players""
SET ""JurisdictionCode"" = 'UK'
WHERE COALESCE(NULLIF(""JurisdictionCode"", ''), '') = '';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore neutral defaults used by the schema migration.
            migrationBuilder.Sql(@"
UPDATE ""Players""
SET
    ""IsSelfExcluded"" = FALSE,
    ""CoolingOffUntilUtc"" = NULL,
    ""JurisdictionCode"" = '',
    ""IsRecommendationRestricted"" = FALSE,
    ""SportMix30d"" = 0.0,
    ""BetTypeMix30d"" = 0.0,
    ""StakeStdDev30d"" = 0.0,
    ""TimeOfDayFitScore"" = 0.0;
");
        }
    }
}
