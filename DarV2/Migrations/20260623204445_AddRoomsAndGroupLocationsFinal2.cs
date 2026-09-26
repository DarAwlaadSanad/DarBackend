using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DarV2.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomsAndGroupLocationsFinal2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM AspNetRoleClaims;");

            migrationBuilder.InsertData(
                table: "AspNetRoleClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "RoleId" },
                values: new object[,]
                {
                    { 1, "Permission", "Permissions.Students.View", "1" },
                    { 2, "Permission", "Permissions.Students.Manage", "1" },
                    { 3, "Permission", "Permissions.Students.Delete", "1" },
                    { 4, "Permission", "Permissions.Groups.View", "1" },
                    { 5, "Permission", "Permissions.Groups.Manage", "1" },
                    { 6, "Permission", "Permissions.Groups.Delete", "1" },
                    { 7, "Permission", "Permissions.Attendance.View", "1" },
                    { 8, "Permission", "Permissions.Attendance.Manage", "1" },
                    { 9, "Permission", "Permissions.Sessions.Manage", "1" },
                    { 10, "Permission", "Permissions.Exams.View", "1" },
                    { 11, "Permission", "Permissions.Exams.Manage", "1" },
                    { 12, "Permission", "Permissions.Competitions.View", "1" },
                    { 13, "Permission", "Permissions.Competitions.Manage", "1" },
                    { 14, "Permission", "Permissions.Memorization.View", "1" },
                    { 15, "Permission", "Permissions.Memorization.Manage", "1" },
                    { 16, "Permission", "Permissions.Fees.View", "1" },
                    { 17, "Permission", "Permissions.Fees.Manage", "1" },
                    { 18, "Permission", "Permissions.Fees.Exempt", "1" },
                    { 19, "Permission", "Permissions.Finance.View", "1" },
                    { 20, "Permission", "Permissions.Finance.Manage", "1" },
                    { 21, "Permission", "Permissions.GroupFees.View", "1" },
                    { 22, "Permission", "Permissions.GroupFees.Manage", "1" },
                    { 23, "Permission", "Permissions.FeePlans.View", "1" },
                    { 24, "Permission", "Permissions.FeePlans.Manage", "1" },
                    { 25, "Permission", "Permissions.Schedules.View", "1" },
                    { 26, "Permission", "Permissions.Schedules.Manage", "1" },
                    { 27, "Permission", "Permissions.AcademicYears.View", "1" },
                    { 28, "Permission", "Permissions.AcademicYears.Manage", "1" },
                    { 29, "Permission", "Permissions.TeacherAttendance.View", "1" },
                    { 30, "Permission", "Permissions.TeacherAttendance.Manage", "1" },
                    { 31, "Permission", "Permissions.Reports.View", "1" },
                    { 32, "Permission", "Permissions.Reports.Export", "1" },
                    { 33, "Permission", "Permissions.Users.View", "1" },
                    { 34, "Permission", "Permissions.Users.Manage", "1" },
                    { 35, "Permission", "Permissions.Roles.View", "1" },
                    { 36, "Permission", "Permissions.Roles.Manage", "1" },
                    { 37, "Permission", "Permissions.TeacherDashboard.View", "1" },
                    { 38, "Permission", "Permissions.Rooms.View", "1" },
                    { 39, "Permission", "Permissions.Rooms.Manage", "1" },
                    { 1000, "Permission", "Permissions.Students.View", "2" },
                    { 1001, "Permission", "Permissions.Groups.View", "2" },
                    { 1002, "Permission", "Permissions.Attendance.View", "2" },
                    { 1003, "Permission", "Permissions.TeacherDashboard.View", "2" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1000);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1001);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1002);

            migrationBuilder.DeleteData(
                table: "AspNetRoleClaims",
                keyColumn: "Id",
                keyValue: 1003);
        }
    }
}
