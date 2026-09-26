using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DarV2.Migrations
{
    public partial class AddStudentPermanentExemption : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FeeExemptionReason",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeeExempted",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeeExemptionReason",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsFeeExempted",
                table: "Students");
        }
    }
}
