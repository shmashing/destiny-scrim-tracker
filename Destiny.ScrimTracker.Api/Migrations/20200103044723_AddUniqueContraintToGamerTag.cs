using Microsoft.EntityFrameworkCore.Migrations;

namespace Destiny.ScrimTracker.Api.Migrations
{
    public partial class AddUniqueContraintToGamerTag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_guardians_GamerTag",
                schema: "scrims_tracker",
                table: "guardians",
                column: "GamerTag",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_guardians_GamerTag",
                schema: "scrims_tracker",
                table: "guardians");
        }
    }
}
