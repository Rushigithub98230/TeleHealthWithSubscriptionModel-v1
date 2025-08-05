using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeQuestionnaireEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerOptions_QuestionOptions_QuestionOptionId",
                table: "UserAnswerOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerOptions_UserAnswers_AnswerId",
                table: "UserAnswerOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResponses_QuestionnaireTemplates_TemplateId",
                table: "UserResponses");

            migrationBuilder.DropTable(
                name: "CategoryQuestionAnswers");

            migrationBuilder.DropTable(
                name: "CategoryQuestions");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswerOptions_QuestionOptionId",
                table: "UserAnswerOptions");

            migrationBuilder.DropColumn(
                name: "QuestionOptionId",
                table: "UserAnswerOptions");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId1",
                table: "UserResponses",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId1",
                table: "QuestionnaireTemplates",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_CategoryId",
                table: "UserResponses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_CategoryId1",
                table: "UserResponses",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_CreatedAt",
                table: "UserResponses",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_IsDeleted",
                table: "UserResponses",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_Status",
                table: "UserResponses",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_UserId",
                table: "UserResponses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_UserId_Status_IsDeleted",
                table: "UserResponses",
                columns: new[] { "UserId", "Status", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UserResponses_UserId_TemplateId",
                table: "UserResponses",
                columns: new[] { "UserId", "TemplateId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserResponses_Status_Valid",
                table: "UserResponses",
                sql: "[Status] IN ('draft', 'completed', 'submitted', 'in_progress', 'reviewed', 'approved', 'rejected')");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_CreatedAt",
                table: "UserAnswers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_IsDeleted",
                table: "UserAnswers",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_ResponseId_IsDeleted",
                table: "UserAnswers",
                columns: new[] { "ResponseId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_ResponseId_QuestionId",
                table: "UserAnswers",
                columns: new[] { "ResponseId", "QuestionId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_UserAnswers_Answer_Type_Valid",
                table: "UserAnswers",
                sql: "([AnswerText] IS NOT NULL) OR ([NumericValue] IS NOT NULL) OR ([DateTimeValue] IS NOT NULL) OR EXISTS (SELECT 1 FROM UserAnswerOptions WHERE AnswerId = Id)");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_AnswerId_OptionId",
                table: "UserAnswerOptions",
                columns: new[] { "AnswerId", "OptionId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_AnswerId_OptionId_IsDeleted",
                table: "UserAnswerOptions",
                columns: new[] { "AnswerId", "OptionId", "IsDeleted" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_CreatedAt",
                table: "UserAnswerOptions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_IsDeleted",
                table: "UserAnswerOptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_IsDeleted",
                table: "Questions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Order",
                table: "Questions",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TemplateId_IsDeleted",
                table: "Questions",
                columns: new[] { "TemplateId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_TemplateId_Order",
                table: "Questions",
                columns: new[] { "TemplateId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Type",
                table: "Questions",
                column: "Type");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Questions_Order_Positive",
                table: "Questions",
                sql: "[Order] > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Questions_Range_Values",
                table: "Questions",
                sql: "([Type] != 'range') OR ([MinValue] IS NULL AND [MaxValue] IS NULL) OR ([MinValue] IS NOT NULL AND [MaxValue] IS NOT NULL AND [MinValue] < [MaxValue])");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_IsCorrect",
                table: "QuestionOptions",
                column: "IsCorrect");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_IsDeleted",
                table: "QuestionOptions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_Order",
                table: "QuestionOptions",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId_IsDeleted",
                table: "QuestionOptions",
                columns: new[] { "QuestionId", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionOptions_QuestionId_Order",
                table: "QuestionOptions",
                columns: new[] { "QuestionId", "Order" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_QuestionOptions_Order_Positive",
                table: "QuestionOptions",
                sql: "[Order] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_CategoryId",
                table: "QuestionnaireTemplates",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_CategoryId_IsActive_IsDeleted",
                table: "QuestionnaireTemplates",
                columns: new[] { "CategoryId", "IsActive", "IsDeleted" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_CategoryId1",
                table: "QuestionnaireTemplates",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_IsActive",
                table: "QuestionnaireTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplates_IsDeleted",
                table: "QuestionnaireTemplates",
                column: "IsDeleted");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireTemplates_Categories_CategoryId",
                table: "QuestionnaireTemplates",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionnaireTemplates_Categories_CategoryId1",
                table: "QuestionnaireTemplates",
                column: "CategoryId1",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerOptions_UserAnswers_AnswerId",
                table: "UserAnswerOptions",
                column: "AnswerId",
                principalTable: "UserAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_Categories_CategoryId",
                table: "UserResponses",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_Categories_CategoryId1",
                table: "UserResponses",
                column: "CategoryId1",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_QuestionnaireTemplates_TemplateId",
                table: "UserResponses",
                column: "TemplateId",
                principalTable: "QuestionnaireTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_Users_UserId",
                table: "UserResponses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireTemplates_Categories_CategoryId",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionnaireTemplates_Categories_CategoryId1",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswerOptions_UserAnswers_AnswerId",
                table: "UserAnswerOptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResponses_Categories_CategoryId",
                table: "UserResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResponses_Categories_CategoryId1",
                table: "UserResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResponses_QuestionnaireTemplates_TemplateId",
                table: "UserResponses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserResponses_Users_UserId",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_CategoryId",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_CategoryId1",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_CreatedAt",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_IsDeleted",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_Status",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_UserId",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_UserId_Status_IsDeleted",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserResponses_UserId_TemplateId",
                table: "UserResponses");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserResponses_Status_Valid",
                table: "UserResponses");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_CreatedAt",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_IsDeleted",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_ResponseId_IsDeleted",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_ResponseId_QuestionId",
                table: "UserAnswers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UserAnswers_Answer_Type_Valid",
                table: "UserAnswers");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswerOptions_AnswerId_OptionId",
                table: "UserAnswerOptions");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswerOptions_AnswerId_OptionId_IsDeleted",
                table: "UserAnswerOptions");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswerOptions_CreatedAt",
                table: "UserAnswerOptions");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswerOptions_IsDeleted",
                table: "UserAnswerOptions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_IsDeleted",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_Order",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_TemplateId_IsDeleted",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_TemplateId_Order",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_Type",
                table: "Questions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Questions_Order_Positive",
                table: "Questions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Questions_Range_Values",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_IsCorrect",
                table: "QuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_IsDeleted",
                table: "QuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_Order",
                table: "QuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_QuestionId_IsDeleted",
                table: "QuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionOptions_QuestionId_Order",
                table: "QuestionOptions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_QuestionOptions_Order_Positive",
                table: "QuestionOptions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireTemplates_CategoryId",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireTemplates_CategoryId_IsActive_IsDeleted",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireTemplates_CategoryId1",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireTemplates_IsActive",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropIndex(
                name: "IX_QuestionnaireTemplates_IsDeleted",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "UserResponses");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "QuestionnaireTemplates");

            migrationBuilder.AddColumn<Guid>(
                name: "QuestionOptionId",
                table: "UserAnswerOptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CategoryQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuestionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryQuestions_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CategoryQuestionAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    AnsweredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryQuestionAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryQuestionAnswers_CategoryQuestions_CategoryQuestionId",
                        column: x => x.CategoryQuestionId,
                        principalTable: "CategoryQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryQuestionAnswers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswerOptions_QuestionOptionId",
                table: "UserAnswerOptions",
                column: "QuestionOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryQuestionAnswers_CategoryQuestionId",
                table: "CategoryQuestionAnswers",
                column: "CategoryQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryQuestionAnswers_UserId",
                table: "CategoryQuestionAnswers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoryQuestions_CategoryId",
                table: "CategoryQuestions",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerOptions_QuestionOptions_QuestionOptionId",
                table: "UserAnswerOptions",
                column: "QuestionOptionId",
                principalTable: "QuestionOptions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswerOptions_UserAnswers_AnswerId",
                table: "UserAnswerOptions",
                column: "AnswerId",
                principalTable: "UserAnswers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAnswers_Questions_QuestionId",
                table: "UserAnswers",
                column: "QuestionId",
                principalTable: "Questions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserResponses_QuestionnaireTemplates_TemplateId",
                table: "UserResponses",
                column: "TemplateId",
                principalTable: "QuestionnaireTemplates",
                principalColumn: "Id");
        }
    }
}
