using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_test.Migrations
{
    /// <inheritdoc />
    public partial class ReplacePlayerProfileWithBettingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_Email",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Players");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Players",
                newName: "PlayerId");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "Players",
                newName: "LastLoginDaysAgo");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "Players",
                newName: "DaysSinceJoined");

            migrationBuilder.RenameIndex(
                name: "IX_Players_Username",
                table: "Players",
                newName: "IX_Players_PlayerId");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageStake",
                table: "Players",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FavouriteTeam",
                table: "Players",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MostBetSport",
                table: "Players",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MostBetType",
                table: "Players",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "Players",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Replace any legacy player rows with canonical records matching the new schema.
            migrationBuilder.Sql("TRUNCATE TABLE \"Players\" RESTART IDENTITY;");

            migrationBuilder.InsertData(
                table: "Players",
                columns: new[]
                {
                    "AverageStake",
                    "CreatedAt",
                    "DaysSinceJoined",
                    "FavouriteTeam",
                    "LastLoginDaysAgo",
                    "MostBetSport",
                    "MostBetType",
                    "PlayerId",
                    "RiskLevel",
                    "UpdatedAt"
                },
                values: new object[,]
                {
                    { 12.50m, new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), 180, "Scotland", 6, "Football", "Bet Builder", "12345", "Low", null },
                    { 8.75m, new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), 60, "Andy Murray", 2, "Tennis", "Accumulator", "12346", "Medium", null },
                    { 25.00m, new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), 400, "N/A", 1, "Horse Racing", "Win", "12347", "High", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AverageStake",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "FavouriteTeam",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "MostBetSport",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "MostBetType",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "Players");

            migrationBuilder.RenameColumn(
                name: "PlayerId",
                table: "Players",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "LastLoginDaysAgo",
                table: "Players",
                newName: "Score");

            migrationBuilder.RenameColumn(
                name: "DaysSinceJoined",
                table: "Players",
                newName: "Level");

            migrationBuilder.RenameIndex(
                name: "IX_Players_PlayerId",
                table: "Players",
                newName: "IX_Players_Username");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Players",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Players_Email",
                table: "Players",
                column: "Email",
                unique: true);
        }
    }
}
