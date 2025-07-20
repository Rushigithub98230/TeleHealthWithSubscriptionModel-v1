using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTelehealth.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPrecisionAndShadowFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppointmentInvitations_InvitationStatuses_InvitationStatusId1",
                table: "AppointmentInvitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_PaymentStatuses_PaymentStatusId1",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_PaymentStatusId1",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_AppointmentInvitations_InvitationStatusId1",
                table: "AppointmentInvitations");

            migrationBuilder.DropColumn(
                name: "PaymentStatusId1",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "InvitationStatusId1",
                table: "AppointmentInvitations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentStatusId1",
                table: "Appointments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InvitationStatusId1",
                table: "AppointmentInvitations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PaymentStatusId1",
                table: "Appointments",
                column: "PaymentStatusId1");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentInvitations_InvitationStatusId1",
                table: "AppointmentInvitations",
                column: "InvitationStatusId1");

            migrationBuilder.AddForeignKey(
                name: "FK_AppointmentInvitations_InvitationStatuses_InvitationStatusId1",
                table: "AppointmentInvitations",
                column: "InvitationStatusId1",
                principalTable: "InvitationStatuses",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_PaymentStatuses_PaymentStatusId1",
                table: "Appointments",
                column: "PaymentStatusId1",
                principalTable: "PaymentStatuses",
                principalColumn: "Id");
        }
    }
}
