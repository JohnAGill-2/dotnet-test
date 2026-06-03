using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_test.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskRecalculationSignals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AverageStakePrevious30d",
                table: "Players",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "BetCount30d",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BetCountPrevious30d",
                table: "Players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastRiskRecalculatedUtc",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxStake30d",
                table: "Players",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RiskChangeReason",
                table: "Players",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageStakePrevious30d",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "BetCount30d",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "BetCountPrevious30d",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "LastRiskRecalculatedUtc",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "MaxStake30d",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "RiskChangeReason",
                table: "Players");
        }
    }
}
