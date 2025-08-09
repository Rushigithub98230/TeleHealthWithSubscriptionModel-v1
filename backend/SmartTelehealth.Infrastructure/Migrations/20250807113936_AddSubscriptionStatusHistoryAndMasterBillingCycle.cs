using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionStatusHistoryAndMasterBillingCycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_ChangedByUserId",
                table: "SubscriptionStatusHistories");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionStatusHistories_ChangedByUserId",
                table: "SubscriptionStatusHistories");

            migrationBuilder.AlterColumn<string>(
                name: "FromStatus",
                table: "SubscriptionStatusHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ChangedByUserId",
                table: "SubscriptionStatusHistories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MasterBillingCycleId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MasterBillingCycleId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DurationInDays",
                table: "MasterBillingCycles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DurationInMonths",
                table: "MasterBillingCycles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StripeInterval",
                table: "MasterBillingCycles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StripeIntervalCount",
                table: "MasterBillingCycles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_MasterBillingCycleId",
                table: "Subscriptions",
                column: "MasterBillingCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_MasterBillingCycleId",
                table: "SubscriptionPlans",
                column: "MasterBillingCycleId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_MasterBillingCycles_MasterBillingCycleId",
                table: "SubscriptionPlans",
                column: "MasterBillingCycleId",
                principalTable: "MasterBillingCycles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_MasterBillingCycles_MasterBillingCycleId",
                table: "Subscriptions",
                column: "MasterBillingCycleId",
                principalTable: "MasterBillingCycles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_MasterBillingCycles_MasterBillingCycleId",
                table: "SubscriptionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_MasterBillingCycles_MasterBillingCycleId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_MasterBillingCycleId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_MasterBillingCycleId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MasterBillingCycleId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "MasterBillingCycleId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "DurationInDays",
                table: "MasterBillingCycles");

            migrationBuilder.DropColumn(
                name: "DurationInMonths",
                table: "MasterBillingCycles");

            migrationBuilder.DropColumn(
                name: "StripeInterval",
                table: "MasterBillingCycles");

            migrationBuilder.DropColumn(
                name: "StripeIntervalCount",
                table: "MasterBillingCycles");

            migrationBuilder.AlterColumn<string>(
                name: "FromStatus",
                table: "SubscriptionStatusHistories",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ChangedByUserId",
                table: "SubscriptionStatusHistories",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionStatusHistories_ChangedByUserId",
                table: "SubscriptionStatusHistories",
                column: "ChangedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionStatusHistories_Users_ChangedByUserId",
                table: "SubscriptionStatusHistories",
                column: "ChangedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
