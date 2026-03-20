using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SoccerTournament.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class seedplayertornaments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Players",
                columns: new[] { "PlayerId", "DateOfBirth", "FirstName", "JerseyNumber", "LastName", "NationalIdNumber", "PlayerPhotoUrl", "Position", "TeamId" },
                values: new object[,]
                {
                    { 1, new DateTime(1995, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "David", 10, "Miller", "1234567890", "https://example.com/players/david_miller.png", "Forward", 1 },
                    { 2, new DateTime(1997, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Emily", 8, "Johnson", "0987654321", "https://example.com/players/emily_johnson.png", "Midfielder", 2 },
                    { 3, new DateTime(1994, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Michael", 5, "Brown", "1122334455", "https://example.com/players/michael_brown.png", "Defender", 3 },
                    { 4, new DateTime(1996, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sarah", 1, "Davis", "5566778899", "https://example.com/players/sarah_davis.png", "Goalkeeper", 4 },
                    { 5, new DateTime(1995, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "John", 9, "Smith", "1234567890", "https://example.com/players/john_smith.png", "Forward", 1 },
                    { 6, new DateTime(1997, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Anna", 7, "Taylor", "0987654321", "https://example.com/players/anna_taylor.png", "Midfielder", 2 },
                    { 7, new DateTime(1994, 12, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "James", 4, "Wilson", "1122334455", "https://example.com/players/james_wilson.png", "Defender", 3 },
                    { 8, new DateTime(1996, 3, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Olivia", 2, "Martinez", "5566778899", "https://example.com/players/olivia_martinez.png", "Goalkeeper", 4 },
                    { 9, new DateTime(1995, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Chris", 11, "Anderson", "1234567890", "https://example.com/players/chris_anderson.png", "Forward", 1 },
                    { 10, new DateTime(1997, 8, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sophia", 6, "Garcia", "0987654321", "https://example.com/players/sophia_garcia.png", "Midfielder", 2 }
                });

            migrationBuilder.InsertData(
                table: "TournamentFormat",
                columns: new[] { "TournamentFormatId", "Description", "DisplayOrder", "FormatName" },
                values: new object[,]
                {
                    { 1, "Teams play each other in a round-robin format. Points are awarded for wins and draws, and the team with the most points at the end of the season wins.", 0, "League" },
                    { 2, "Teams are paired in a single-elimination format. The winner of each match advances to the next round, while the loser is eliminated from the tournament.", 0, "Knockout" },
                    { 3, "Teams are divided into groups, and each team plays against the others in their group. The top teams from each group advance to the knockout stage.", 0, "Group Stage" },
                    { 4, "Teams play each other in a round-robin format. Points are awarded for wins and draws, and the team with the most points at the end of the season wins.", 0, "Single Elimination" },
                    { 5, "Teams are paired in a single-elimination format. The winner of each match advances to the next round, while the loser is eliminated from the tournament.", 0, "Double Elimination" },
                    { 6, "Teams are divided into groups, and each team plays against the others in their group. The top teams from each group advance to the knockout stage.", 0, "Round Robin" }
                });

            migrationBuilder.InsertData(
                table: "Tournaments",
                columns: new[] { "TournamentId", "DeadlineDate", "Description", "EndDate", "EntryFee", "Format", "Location", "MaxParticipants", "Name", "PointsDraw", "PointsLoss", "PointsWin", "Season", "StartDate", "Status", "TeamId" },
                values: new object[,]
                {
                    { 1, null, null, null, null, "League", "Main Stadium", null, "Champions Summer Cup", 1, 0, 3, "", new DateTime(2026, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Live", null },
                    { 2, null, null, null, null, "Knockout", "Sandton Field", null, "Gauteng Amateur League", 1, 0, 3, "", new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Upcoming", null },
                    { 3, null, null, null, null, "Group Stage", "The Arena", null, "Midnight Pro Series", 1, 0, 3, "", new DateTime(2026, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Registration Open", null },
                    { 4, null, null, null, null, "League", "Main Stadium", null, "Annual spring", 1, 0, 3, "", new DateTime(2026, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Live", null },
                    { 5, null, null, null, null, "Knockout", "Sandton Field", null, "featuring top regional teams", 1, 0, 3, "", new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Upcoming", null },
                    { 6, null, null, null, null, "Group Stage", "The Arena", null, "competing", 1, 0, 3, "", new DateTime(2026, 10, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Registration Open", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TournamentFormat",
                keyColumn: "TournamentFormatId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TournamentFormat",
                keyColumn: "TournamentFormatId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TournamentFormat",
                keyColumn: "TournamentFormatId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TournamentFormat",
                keyColumn: "TournamentFormatId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TournamentFormat",
                keyColumn: "TournamentFormatId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TournamentFormat",
                keyColumn: "TournamentFormatId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "TournamentId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "TournamentId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "TournamentId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "TournamentId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "TournamentId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "TournamentId",
                keyValue: 6);
        }
    }
}
