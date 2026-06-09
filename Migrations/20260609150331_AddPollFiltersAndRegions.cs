using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fifa_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPollFiltersAndRegions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegionFilter",
                table: "VotingSessions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Region",
                table: "Teams",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VotingSessionTeams",
                columns: table => new
                {
                    TeamsId = table.Column<int>(type: "int", nullable: false),
                    VotingSessionsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotingSessionTeams", x => new { x.TeamsId, x.VotingSessionsId });
                    table.ForeignKey(
                        name: "FK_VotingSessionTeams_Teams_TeamsId",
                        column: x => x.TeamsId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VotingSessionTeams_VotingSessions_VotingSessionsId",
                        column: x => x.VotingSessionsId,
                        principalTable: "VotingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_VotingSessionTeams_VotingSessionsId",
                table: "VotingSessionTeams",
                column: "VotingSessionsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VotingSessionTeams");

            migrationBuilder.DropColumn(
                name: "RegionFilter",
                table: "VotingSessions");

            migrationBuilder.DropColumn(
                name: "Region",
                table: "Teams");
        }
    }
}
