using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fifa_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddWinnersCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WinnersCount",
                table: "VotingSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WinnersCount",
                table: "VotingSessions");
        }
    }
}
