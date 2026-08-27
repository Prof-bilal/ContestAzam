using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventSphere.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSuspendReasonToAppUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SuspendReason",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuspendReason",
                table: "Users");
        }
    }
}
