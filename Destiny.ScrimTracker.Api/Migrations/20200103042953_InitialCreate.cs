using Microsoft.EntityFrameworkCore.Migrations;

namespace Destiny.ScrimTracker.Api.Migrations
{
    public partial class InitialCreate : Migration
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
                    LifetimeDeaths = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guardians", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "guardians",
                schema: "scrims_tracker");
        }
    }
}
