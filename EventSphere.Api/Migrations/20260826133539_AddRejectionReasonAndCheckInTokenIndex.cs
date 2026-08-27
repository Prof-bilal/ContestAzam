using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventSphere.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRejectionReasonAndCheckInTokenIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Events",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrations_CheckInToken",
                table: "Registrations",
                column: "CheckInToken",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Registrations_CheckInToken",
                table: "Registrations");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Events");
        }
    }
}
