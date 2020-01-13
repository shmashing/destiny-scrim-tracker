using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Destiny.ScrimTracker.Api.Migrations
{
    public partial class UpdateEfficiencyModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                schema: "scrims_tracker",
                table: "guardian_efficiencies",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateIndex(
                name: "IX_guardian_efficiencies_GuardianId",
                schema: "scrims_tracker",
                table: "guardian_efficiencies",
                column: "GuardianId");

            migrationBuilder.AddForeignKey(
                name: "FK_guardian_efficiencies_guardians_GuardianId",
                schema: "scrims_tracker",
                table: "guardian_efficiencies",
                column: "GuardianId",
                principalSchema: "scrims_tracker",
                principalTable: "guardians",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_guardian_efficiencies_guardians_GuardianId",
                schema: "scrims_tracker",
                table: "guardian_efficiencies");

            migrationBuilder.DropIndex(
                name: "IX_guardian_efficiencies_GuardianId",
                schema: "scrims_tracker",
                table: "guardian_efficiencies");

            migrationBuilder.AlterColumn<DateTime>(
                name: "TimeStamp",
                schema: "scrims_tracker",
                table: "guardian_efficiencies",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldDefaultValueSql: "now()");
        }
    }
}
