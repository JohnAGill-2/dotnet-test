using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_test.Migrations
{
    /// <inheritdoc />
    public partial class AddRecommendationSignalsAndGuardrails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "BetTypeMix30d",
                table: "Players",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CoolingOffUntilUtc",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecommendationRestricted",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSelfExcluded",
                table: "Players",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JurisdictionCode",
                table: "Players",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "SportMix30d",
                table: "Players",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "StakeStdDev30d",
                table: "Players",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "TimeOfDayFitScore",
                table: "Players",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BetTypeMix30d",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "CoolingOffUntilUtc",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "IsRecommendationRestricted",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "IsSelfExcluded",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "JurisdictionCode",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "SportMix30d",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "StakeStdDev30d",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "TimeOfDayFitScore",
                table: "Players");
        }
    }
}
