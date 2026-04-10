using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCG.InquiryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BatchMultiNationality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NationalityCode",
                schema: "inquiry",
                table: "Batches");

            migrationBuilder.AddColumn<string>(
                name: "DepartureCountry",
                schema: "inquiry",
                table: "BatchTravelers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FlightNumber",
                schema: "inquiry",
                table: "BatchTravelers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PassportExpiry",
                schema: "inquiry",
                table: "BatchTravelers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PurposeOfTravel",
                schema: "inquiry",
                table: "BatchTravelers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "inquiry",
                table: "Batches",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartureCountry",
                schema: "inquiry",
                table: "BatchTravelers");

            migrationBuilder.DropColumn(
                name: "FlightNumber",
                schema: "inquiry",
                table: "BatchTravelers");

            migrationBuilder.DropColumn(
                name: "PassportExpiry",
                schema: "inquiry",
                table: "BatchTravelers");

            migrationBuilder.DropColumn(
                name: "PurposeOfTravel",
                schema: "inquiry",
                table: "BatchTravelers");

            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "inquiry",
                table: "Batches");

            migrationBuilder.AddColumn<string>(
                name: "NationalityCode",
                schema: "inquiry",
                table: "Batches",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }
    }
}
