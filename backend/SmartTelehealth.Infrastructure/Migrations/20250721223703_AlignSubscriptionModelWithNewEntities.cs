using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignSubscriptionModelWithNewEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResumedAt",
                table: "Subscriptions",
                newName: "TrialStartDate");

            migrationBuilder.RenameColumn(
                name: "PausedAt",
                table: "Subscriptions",
                newName: "TrialEndDate");

            migrationBuilder.AddColumn<int>(
                name: "AllowedValue",
                table: "UserSubscriptionPrivilegeUsages",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedAt",
                table: "UserSubscriptionPrivilegeUsages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "UserSubscriptionPrivilegeUsages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetAt",
                table: "UserSubscriptionPrivilegeUsages",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StatusReason",
                table: "Subscriptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FailedPaymentAttempts",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrialSubscription",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastPaymentError",
                table: "Subscriptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPaymentFailedDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Subscriptions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethodId",
                table: "Subscriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResumedDate",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalUsageCount",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TrialDurationInDays",
                table: "Subscriptions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "StripeQuarterlyPriceId",
                table: "SubscriptionPlans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripeProductId",
                table: "SubscriptionPlans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripeMonthlyPriceId",
                table: "SubscriptionPlans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripeAnnualPriceId",
                table: "SubscriptionPlans",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubscriptionPlans",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountValidUntil",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountedPrice",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "SubscriptionPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Features",
                table: "SubscriptionPlans",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTrialAllowed",
                table: "SubscriptionPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ShortDescription",
                table: "SubscriptionPlans",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Terms",
                table: "SubscriptionPlans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrialDurationInDays",
                table: "SubscriptionPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SubscriptionPlanPrivileges",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveDate",
                table: "SubscriptionPlanPrivileges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "SubscriptionPlanPrivileges",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SubscriptionPlanPrivileges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentId",
                table: "SubscriptionPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripeInvoiceId",
                table: "SubscriptionPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiptUrl",
                table: "SubscriptionPayments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentIntentId",
                table: "SubscriptionPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceId",
                table: "SubscriptionPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FailureReason",
                table: "SubscriptionPayments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubscriptionPayments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "AttemptCount",
                table: "SubscriptionPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BillingPeriodEnd",
                table: "SubscriptionPayments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "BillingPeriodStart",
                table: "SubscriptionPayments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FailedAt",
                table: "SubscriptionPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "NetAmount",
                table: "SubscriptionPayments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRetryAt",
                table: "SubscriptionPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                table: "SubscriptionPayments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "SubscriptionPayments",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "SubscriptionPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CategoryQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QuestionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OptionsJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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
                name: "PaymentRefunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StripeRefundId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RefundedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRefunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_SubscriptionPayments_SubscriptionPaymentId",
                        column: x => x.SubscriptionPaymentId,
                        principalTable: "SubscriptionPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentRefunds_Users_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionStatusHistories_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SubscriptionStatusHistories_Users_ChangedByUserId",
                        column: x => x.ChangedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_ProcessedByUserId",
                table: "PaymentRefunds",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRefunds_SubscriptionPaymentId",
                table: "PaymentRefunds",
                column: "SubscriptionPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_ChangedByUserId",
                table: "SubscriptionStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_SubscriptionId",
                table: "SubscriptionStatusHistories",
                column: "SubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryQuestionAnswers");

            migrationBuilder.DropTable(
                name: "PaymentRefunds");

            migrationBuilder.DropTable(
                name: "SubscriptionStatusHistories");

            migrationBuilder.DropTable(
                name: "CategoryQuestions");

            migrationBuilder.DropColumn(
                name: "AllowedValue",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "LastUsedAt",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "ResetAt",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "FailedPaymentAttempts",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "IsTrialSubscription",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "LastPaymentDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "LastPaymentError",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "LastPaymentFailedDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "LastUsedDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentMethodId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ResumedDate",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TotalUsageCount",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TrialDurationInDays",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "DiscountValidUntil",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "DiscountedPrice",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "Features",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "IsTrialAllowed",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "ShortDescription",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "Terms",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "TrialDurationInDays",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "EffectiveDate",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "AttemptCount",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "BillingPeriodEnd",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "BillingPeriodStart",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "FailedAt",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "NetAmount",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "NextRetryAt",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "RefundedAmount",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "SubscriptionPayments");

            migrationBuilder.RenameColumn(
                name: "TrialStartDate",
                table: "Subscriptions",
                newName: "ResumedAt");

            migrationBuilder.RenameColumn(
                name: "TrialEndDate",
                table: "Subscriptions",
                newName: "PausedAt");

            migrationBuilder.AlterColumn<string>(
                name: "StatusReason",
                table: "Subscriptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripeQuarterlyPriceId",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripeProductId",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripeMonthlyPriceId",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripeAnnualPriceId",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubscriptionPlans",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentId",
                table: "SubscriptionPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripeInvoiceId",
                table: "SubscriptionPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReceiptUrl",
                table: "SubscriptionPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentIntentId",
                table: "SubscriptionPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceId",
                table: "SubscriptionPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FailureReason",
                table: "SubscriptionPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "SubscriptionPayments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
