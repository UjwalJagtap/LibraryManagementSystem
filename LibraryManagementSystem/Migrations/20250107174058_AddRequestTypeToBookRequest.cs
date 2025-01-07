using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestTypeToBookRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsRenewalRequest",
                table: "BookRequests",
                newName: "RequestType");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$50Q8La.WJK0X9XBpPLRtb.Qu2poXLIU5GHGgCoqQRfIR2dwc8ZRBe");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestType",
                table: "BookRequests",
                newName: "IsRenewalRequest");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserId",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$yIs46VwQ0ZOPJ0aiupRJ3eUkl4vuiIQih0WJtDv8IMgLzBRydsQae");
        }
    }
}
