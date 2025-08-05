using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConvertQuestionTypeToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserResponses_Status_Valid",
                table: "UserResponses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Questions_Range_Values",
                table: "Questions");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "UserResponses",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Questions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserResponses_Status_Valid",
                table: "UserResponses",
                sql: "[Status] IN (1, 2, 3, 4, 5, 6, 7)");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Questions_Range_Values",
                table: "Questions",
                sql: "([Type] != 6) OR ([MinValue] IS NULL AND [MaxValue] IS NULL) OR ([MinValue] IS NOT NULL AND [MaxValue] IS NOT NULL AND [MinValue] < [MaxValue])");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UserResponses_Status_Valid",
                table: "UserResponses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Questions_Range_Values",
                table: "Questions");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "UserResponses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Questions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserResponses_Status_Valid",
                table: "UserResponses",
                sql: "[Status] IN ('draft', 'completed', 'submitted', 'in_progress', 'reviewed', 'approved', 'rejected')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Questions_Range_Values",
                table: "Questions",
                sql: "([Type] != 'range') OR ([MinValue] IS NULL AND [MaxValue] IS NULL) OR ([MinValue] IS NOT NULL AND [MaxValue] IS NOT NULL AND [MinValue] < [MaxValue])");
        }
    }
}
