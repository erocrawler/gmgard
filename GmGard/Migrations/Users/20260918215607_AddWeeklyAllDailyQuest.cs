using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GmGard.Migrations.Users
{
    /// <inheritdoc />
    public partial class AddWeeklyAllDailyQuest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastAllDailyDate",
                table: "UserQuests",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WeekAllDailyCount",
                table: "UserQuests",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastAllDailyDate",
                table: "UserQuests");

            migrationBuilder.DropColumn(
                name: "WeekAllDailyCount",
                table: "UserQuests");
        }
    }
}
