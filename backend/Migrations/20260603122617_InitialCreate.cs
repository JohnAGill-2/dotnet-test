using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace dotnet_test.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DaysSinceJoined = table.Column<int>(type: "integer", nullable: false),
                    MostBetSport = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FavouriteTeam = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MostBetType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    AverageStake = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    LastLoginDaysAgo = table.Column<int>(type: "integer", nullable: false),
                    RiskLevel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsSelfExcluded = table.Column<bool>(type: "boolean", nullable: false),
                    CoolingOffUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    JurisdictionCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsRecommendationRestricted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_PlayerId",
                table: "Players",
                column: "PlayerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
