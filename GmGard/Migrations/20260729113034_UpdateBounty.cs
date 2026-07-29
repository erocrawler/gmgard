using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GmGard.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBounty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CloseDate",
                table: "Bounties",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Deposit",
                table: "Bounties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiresAt",
                table: "Bounties",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HelpfulReward",
                table: "Bounties",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsHelpful",
                table: "Answers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseDate",
                table: "Bounties");

            migrationBuilder.DropColumn(
                name: "Deposit",
                table: "Bounties");

            migrationBuilder.DropColumn(
                name: "ExpiresAt",
                table: "Bounties");

            migrationBuilder.DropColumn(
                name: "HelpfulReward",
                table: "Bounties");

            migrationBuilder.DropColumn(
                name: "IsHelpful",
                table: "Answers");
        }
    }
}
