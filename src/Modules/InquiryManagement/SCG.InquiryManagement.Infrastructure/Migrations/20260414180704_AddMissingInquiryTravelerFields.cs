using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCG.InquiryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingInquiryTravelerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartureCountry",
                schema: "inquiry",
                table: "Inquiries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                schema: "inquiry",
                table: "Inquiries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportExpiry",
                schema: "inquiry",
                table: "Inquiries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PurposeOfTravel",
                schema: "inquiry",
                table: "Inquiries",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureCountry",
                schema: "inquiry",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "FlightNumber",
                schema: "inquiry",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "PassportExpiry",
                schema: "inquiry",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "PurposeOfTravel",
                schema: "inquiry",
                table: "Inquiries");
        }
    }
}
