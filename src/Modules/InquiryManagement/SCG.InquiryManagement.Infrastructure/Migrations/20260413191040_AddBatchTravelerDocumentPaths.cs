using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCG.InquiryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBatchTravelerDocumentPaths : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PassportImageDocumentPath",
                schema: "inquiry",
                table: "BatchTravelers",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TicketImageDocumentPath",
                schema: "inquiry",
                table: "BatchTravelers",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PassportImageDocumentPath",
                schema: "inquiry",
                table: "BatchTravelers");

            migrationBuilder.DropColumn(
                name: "TicketImageDocumentPath",
                schema: "inquiry",
                table: "BatchTravelers");
        }
    }
}
