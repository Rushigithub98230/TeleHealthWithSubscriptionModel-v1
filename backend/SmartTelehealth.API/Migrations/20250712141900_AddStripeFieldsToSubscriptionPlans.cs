using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.API.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeFieldsToSubscriptionPlans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripeAnnualPriceId",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeMonthlyPriceId",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeProductId",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeQuarterlyPriceId",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripeAnnualPriceId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "StripeMonthlyPriceId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "StripeProductId",
                table: "SubscriptionPlans");

            migrationBuilder.DropColumn(
                name: "StripeQuarterlyPriceId",
                table: "SubscriptionPlans");
        }
    }
}
