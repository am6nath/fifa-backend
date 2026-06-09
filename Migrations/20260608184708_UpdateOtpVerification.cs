using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace fifa_backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOtpVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "OtpVerifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                table: "OtpVerifications",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "OtpVerifications");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                table: "OtpVerifications");
        }
    }
}
