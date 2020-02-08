using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Destiny.ScrimTracker.App.Migrations
{
    public partial class InitialCreation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scrims_tracker");

            migrationBuilder.CreateTable(
                name: "guardians",
                schema: "scrims_tracker",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    GamerTag = table.Column<string>(nullable: false),
                    LifetimeKills = table.Column<int>(nullable: false),
                    LifetimeDeaths = table.Column<int>(nullable: false),
                    EloModifier = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guardians", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "matches",
                schema: "scrims_tracker",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false, defaultValueSql: "now()"),
                    MatchType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "guardian_efficiencies",
                schema: "scrims_tracker",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    MatchId = table.Column<string>(nullable: true),
                    GuardianId = table.Column<string>(nullable: true),
                    PreviousEfficiency = table.Column<double>(nullable: false),
                    NewEfficiency = table.Column<double>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guardian_efficiencies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guardian_efficiencies_guardians_GuardianId",
                        column: x => x.GuardianId,
                        principalSchema: "scrims_tracker",
                        principalTable: "guardians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "guardian_elos",
                schema: "scrims_tracker",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    MatchId = table.Column<string>(nullable: true),
                    GuardianId = table.Column<string>(nullable: true),
                    PreviousElo = table.Column<double>(nullable: false),
                    NewElo = table.Column<double>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guardian_elos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guardian_elos_guardians_GuardianId",
                        column: x => x.GuardianId,
                        principalSchema: "scrims_tracker",
                        principalTable: "guardians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "match_teams",
                schema: "scrims_tracker",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    MatchId = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    TeamScore = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_match_teams_matches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "scrims_tracker",
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "guardian_match_results",
                schema: "scrims_tracker",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    GuardianId = table.Column<string>(nullable: true),
                    GuardianName = table.Column<string>(nullable: true),
                    MatchId = table.Column<string>(nullable: true),
                    MatchTeamId = table.Column<string>(nullable: true),
                    Efficiency = table.Column<double>(nullable: false),
                    Kills = table.Column<int>(nullable: false),
                    Deaths = table.Column<int>(nullable: false),
                    Assists = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guardian_match_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guardian_match_results_matches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "scrims_tracker",
                        principalTable: "matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_guardian_match_results_match_teams_MatchTeamId",
                        column: x => x.MatchTeamId,
                        principalSchema: "scrims_tracker",
                        principalTable: "match_teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_guardian_efficiencies_GuardianId",
                schema: "scrims_tracker",
                table: "guardian_efficiencies",
                column: "GuardianId");

            migrationBuilder.CreateIndex(
                name: "IX_guardian_elos_GuardianId",
                schema: "scrims_tracker",
                table: "guardian_elos",
                column: "GuardianId");

            migrationBuilder.CreateIndex(
                name: "IX_guardian_match_results_MatchId",
                schema: "scrims_tracker",
                table: "guardian_match_results",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_guardian_match_results_MatchTeamId",
                schema: "scrims_tracker",
                table: "guardian_match_results",
                column: "MatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_guardians_GamerTag",
                schema: "scrims_tracker",
                table: "guardians",
                column: "GamerTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_match_teams_MatchId",
                schema: "scrims_tracker",
                table: "match_teams",
                column: "MatchId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guardian_efficiencies",
                schema: "scrims_tracker");

            migrationBuilder.DropTable(
                name: "guardian_elos",
                schema: "scrims_tracker");

            migrationBuilder.DropTable(
                name: "guardian_match_results",
                schema: "scrims_tracker");

            migrationBuilder.DropTable(
                name: "guardians",
                schema: "scrims_tracker");

            migrationBuilder.DropTable(
                name: "match_teams",
                schema: "scrims_tracker");

            migrationBuilder.DropTable(
                name: "matches",
                schema: "scrims_tracker");
        }
    }
}
