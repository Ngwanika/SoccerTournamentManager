using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoccerTournament.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class matchesseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AwayScore",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AwayTeamId",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HomeScore",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HomeTeamId",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Stage",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Matches",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_AwayTeamId",
                table: "Matches",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_HomeTeamId",
                table: "Matches",
                column: "HomeTeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Teams_AwayTeamId",
                table: "Matches",
                column: "AwayTeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Teams_HomeTeamId",
                table: "Matches",
                column: "HomeTeamId",
                principalTable: "Teams",
                principalColumn: "TeamId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Teams_AwayTeamId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Teams_HomeTeamId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_AwayTeamId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_HomeTeamId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "AwayScore",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "AwayTeamId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "HomeScore",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "HomeTeamId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Stage",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Matches");
        }
    }
}
