using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignWithNewSubscriptionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_Categories_CategoryId",
                table: "SubscriptionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "BillingFrequency",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "IsPaused",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "AnnualPrice",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "MonthlyPrice",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "DataType",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "DefaultValue",
                table: "Privileges");

            migrationBuilder.RenameColumn(
                name: "QuarterlyPrice",
                table: "SubscriptionPlans",
                newName: "Price");

            migrationBuilder.AlterColumn<int>(
                name: "UsedValue",
                table: "UserSubscriptionPrivilegeUsages",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<Guid>(
                name: "PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "SubscriptionPlanPrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UsagePeriodEnd",
                table: "UserSubscriptionPrivilegeUsages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UsagePeriodStart",
                table: "UserSubscriptionPrivilegeUsages",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Subscriptions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "BillingCycleId",
                table: "Subscriptions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                table: "Subscriptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "BillingCycleId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "Value",
                table: "SubscriptionPlanPrivileges",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<int>(
                name: "DurationMonths",
                table: "SubscriptionPlanPrivileges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                table: "SubscriptionPayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PrivilegeTypeId",
                table: "Privileges",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "AccrualEndDate",
                table: "BillingRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccrualStartDate",
                table: "BillingRecords",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AccruedAmount",
                table: "BillingRecords",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyId",
                table: "BillingRecords",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "MasterBillingCycles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterBillingCycles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterCurrencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterCurrencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MasterPrivilegeTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterPrivilegeTypes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                column: "SubscriptionPlanPrivilegeId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_BillingCycleId",
                table: "Subscriptions",
                column: "BillingCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_BillingCycleId",
                table: "SubscriptionPlans",
                column: "BillingCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlans_CurrencyId",
                table: "SubscriptionPlans",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPlanPrivileges_UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                column: "UsagePeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayments_CurrencyId",
                table: "SubscriptionPayments",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Privileges_PrivilegeTypeId",
                table: "Privileges",
                column: "PrivilegeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_BillingRecords_CurrencyId",
                table: "BillingRecords",
                column: "CurrencyId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingRecords_MasterCurrencies_CurrencyId",
                table: "BillingRecords",
                column: "CurrencyId",
                principalTable: "MasterCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId",
                table: "Privileges",
                column: "PrivilegeTypeId",
                principalTable: "MasterPrivilegeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPayments_MasterCurrencies_CurrencyId",
                table: "SubscriptionPayments",
                column: "CurrencyId",
                principalTable: "MasterCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                table: "SubscriptionPlanPrivileges",
                column: "UsagePeriodId",
                principalTable: "MasterBillingCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_Categories_CategoryId",
                table: "SubscriptionPlans",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_MasterBillingCycles_BillingCycleId",
                table: "SubscriptionPlans",
                column: "BillingCycleId",
                principalTable: "MasterBillingCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_MasterCurrencies_CurrencyId",
                table: "SubscriptionPlans",
                column: "CurrencyId",
                principalTable: "MasterCurrencies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_MasterBillingCycles_BillingCycleId",
                table: "Subscriptions",
                column: "BillingCycleId",
                principalTable: "MasterBillingCycles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                column: "PrivilegeId",
                principalTable: "Privileges",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivileges_SubscriptionPlanPrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                column: "SubscriptionPlanPrivilegeId",
                principalTable: "SubscriptionPlanPrivileges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingRecords_MasterCurrencies_CurrencyId",
                table: "BillingRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Privileges_MasterPrivilegeTypes_PrivilegeTypeId",
                table: "Privileges");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPayments_MasterCurrencies_CurrencyId",
                table: "SubscriptionPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlanPrivileges_MasterBillingCycles_UsagePeriodId",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_Categories_CategoryId",
                table: "SubscriptionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_MasterBillingCycles_BillingCycleId",
                table: "SubscriptionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionPlans_MasterCurrencies_CurrencyId",
                table: "SubscriptionPlans");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_MasterBillingCycles_BillingCycleId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivileges_SubscriptionPlanPrivilegeId",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropTable(
                name: "MasterBillingCycles");

            migrationBuilder.DropTable(
                name: "MasterCurrencies");

            migrationBuilder.DropTable(
                name: "MasterPrivilegeTypes");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptionPrivilegeUsages_SubscriptionPlanPrivilegeId",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_BillingCycleId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_BillingCycleId",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlans_CurrencyId",
                table: "SubscriptionPlans");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPlanPrivileges_UsagePeriodId",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionPayments_CurrencyId",
                table: "SubscriptionPayments");

            migrationBuilder.DropIndex(
                name: "IX_Privileges_PrivilegeTypeId",
                table: "Privileges");

            migrationBuilder.DropIndex(
                name: "IX_BillingRecords_CurrencyId",
                table: "BillingRecords");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanPrivilegeId",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "UsagePeriodEnd",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "UsagePeriodStart",
                table: "UserSubscriptionPrivilegeUsages");

            migrationBuilder.DropColumn(
                name: "BillingCycleId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "StatusReason",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "BillingCycleId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "DurationMonths",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "UsagePeriodId",
                table: "SubscriptionPlanPrivileges");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "SubscriptionPayments");

            migrationBuilder.DropColumn(
                name: "PrivilegeTypeId",
                table: "Privileges");

            migrationBuilder.DropColumn(
                name: "AccrualEndDate",
                table: "BillingRecords");

            migrationBuilder.DropColumn(
                name: "AccrualStartDate",
                table: "BillingRecords");

            migrationBuilder.DropColumn(
                name: "AccruedAmount",
                table: "BillingRecords");

            migrationBuilder.DropColumn(
                name: "CurrencyId",
                table: "BillingRecords");

            migrationBuilder.RenameColumn(
                name: "Price",
                table: "SubscriptionPlans",
                newName: "QuarterlyPrice");

            migrationBuilder.AlterColumn<string>(
                name: "UsedValue",
                table: "UserSubscriptionPrivilegeUsages",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<Guid>(
                name: "PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "BillingFrequency",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaused",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "SubscriptionPlans",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AnnualPrice",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyPrice",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "SubscriptionPlanPrivileges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "SubscriptionPayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DataType",
                table: "Privileges",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DefaultValue",
                table: "Privileges",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionPlans_Categories_CategoryId",
                table: "SubscriptionPlans",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptionPrivilegeUsages_Privileges_PrivilegeId",
                table: "UserSubscriptionPrivilegeUsages",
                column: "PrivilegeId",
                principalTable: "Privileges",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
