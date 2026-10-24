using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoccerTournament.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class TournamentOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "WhatsAppGroupLink",
                table: "Teams",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Players",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedOfficialUserId",
                table: "Matches",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssistantOfficialUserId",
                table: "Matches",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BracketPosition",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BracketRound",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WinnerTeamId",
                table: "Matches",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MatchAuditLogs",
                columns: table => new
                {
                    MatchAuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ActorUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ActorName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchAuditLogs", x => x.MatchAuditLogId);
                    table.ForeignKey(
                        name: "FK_MatchAuditLogs_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchEvents",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MatchId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: true),
                    TeamId = table.Column<int>(type: "int", nullable: true),
                    EventType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EventMinute = table.Column<int>(type: "int", nullable: false),
                    RecordedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchEvents", x => x.EventId);
                    table.ForeignKey(
                        name: "FK_MatchEvents_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MatchEvents_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatchEvents_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId");
                    table.ForeignKey(
                        name: "FK_MatchEvents_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId");
                });

            migrationBuilder.CreateTable(
                name: "TeamContactMessages",
                columns: table => new
                {
                    TeamContactMessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeamId = table.Column<int>(type: "int", nullable: false),
                    SenderUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ReplyEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamContactMessages", x => x.TeamContactMessageId);
                    table.ForeignKey(
                        name: "FK_TeamContactMessages_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "TeamId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TournamentDraws",
                columns: table => new
                {
                    TournamentDrawId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TournamentId = table.Column<int>(type: "int", nullable: false),
                    TeamOrderJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    KnockoutTeamOrderJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FinalizedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinalizedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TournamentDraws", x => x.TournamentDrawId);
                    table.ForeignKey(
                        name: "FK_TournamentDraws_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "TournamentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 1,
                column: "Email",
                value: null);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 2,
                column: "Email",
                value: null);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 3,
                column: "Email",
                value: null);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 4,
                column: "Email",
                value: null);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 5,
                column: "Email",
                value: null);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 6,
                column: "Email",
                value: null);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 7,
                column: "Email",
                value: null);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 8,
                column: "Email",
                value: null);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 9,
                column: "Email",
                value: null);

            migrationBuilder.UpdateData(
                table: "Players",
                keyColumn: "PlayerId",
                keyValue: 10,
                column: "Email",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teams",
                keyColumn: "TeamId",
                keyValue: 1,
                column: "WhatsAppGroupLink",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teams",
                keyColumn: "TeamId",
                keyValue: 2,
                column: "WhatsAppGroupLink",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teams",
                keyColumn: "TeamId",
                keyValue: 3,
                column: "WhatsAppGroupLink",
                value: null);

            migrationBuilder.UpdateData(
                table: "Teams",
                keyColumn: "TeamId",
                keyValue: 4,
                column: "WhatsAppGroupLink",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Matches_AssignedOfficialUserId",
                table: "Matches",
                column: "AssignedOfficialUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_AssistantOfficialUserId",
                table: "Matches",
                column: "AssistantOfficialUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_TournamentId",
                table: "Matches",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchAuditLogs_MatchId_CreatedAtUtc",
                table: "MatchAuditLogs",
                columns: new[] { "MatchId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_MatchId_EventMinute",
                table: "MatchEvents",
                columns: new[] { "MatchId", "EventMinute" });

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_PlayerId",
                table: "MatchEvents",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_RecordedByUserId",
                table: "MatchEvents",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEvents_TeamId",
                table: "MatchEvents",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamContactMessages_SentAtUtc",
                table: "TeamContactMessages",
                column: "SentAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TeamContactMessages_TeamId",
                table: "TeamContactMessages",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TournamentDraws_TournamentId",
                table: "TournamentDraws",
                column: "TournamentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_AspNetUsers_AssignedOfficialUserId",
                table: "Matches",
                column: "AssignedOfficialUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_AspNetUsers_AssistantOfficialUserId",
                table: "Matches",
                column: "AssistantOfficialUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Tournaments_TournamentId",
                table: "Matches",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "TournamentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_AspNetUsers_AssignedOfficialUserId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_AspNetUsers_AssistantOfficialUserId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Tournaments_TournamentId",
                table: "Matches");

            migrationBuilder.DropTable(
                name: "MatchAuditLogs");

            migrationBuilder.DropTable(
                name: "MatchEvents");

            migrationBuilder.DropTable(
                name: "TeamContactMessages");

            migrationBuilder.DropTable(
                name: "TournamentDraws");

            migrationBuilder.DropIndex(
                name: "IX_Matches_AssignedOfficialUserId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_AssistantOfficialUserId",
                table: "Matches");

            migrationBuilder.DropIndex(
                name: "IX_Matches_TournamentId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "WhatsAppGroupLink",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "AssignedOfficialUserId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "AssistantOfficialUserId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "BracketPosition",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "BracketRound",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "WinnerTeamId",
                table: "Matches");
        }
    }
}
