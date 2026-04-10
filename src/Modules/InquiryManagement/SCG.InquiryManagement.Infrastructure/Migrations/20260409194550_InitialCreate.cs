using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCG.InquiryManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inquiry");

            migrationBuilder.CreateTable(
                name: "Batches",
                schema: "inquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    InquiryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NationalityCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TravelerCount = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TotalFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inquiries",
                schema: "inquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InquiryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FirstNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PassportNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NationalityCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TravelDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalAirport = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransitCountries = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ResultCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DocumentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquiries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BatchTravelers",
                schema: "inquiry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastNameEn = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LastNameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PassportNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NationalityCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TravelDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalAirport = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TransitCountries = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowIndex = table.Column<int>(type: "int", nullable: false),
                    InquiryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatchTravelers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BatchTravelers_Batches_BatchId",
                        column: x => x.BatchId,
                        principalSchema: "inquiry",
                        principalTable: "Batches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatchTravelers_Inquiries_InquiryId",
                        column: x => x.InquiryId,
                        principalSchema: "inquiry",
                        principalTable: "Inquiries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Batches_AgencyId",
                schema: "inquiry",
                table: "Batches",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_CreatedAt",
                schema: "inquiry",
                table: "Batches",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Batches_Status",
                schema: "inquiry",
                table: "Batches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BatchTravelers_BatchId_RowIndex",
                schema: "inquiry",
                table: "BatchTravelers",
                columns: new[] { "BatchId", "RowIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_BatchTravelers_InquiryId",
                schema: "inquiry",
                table: "BatchTravelers",
                column: "InquiryId");

            migrationBuilder.CreateIndex(
                name: "IX_BatchTravelers_PassportNumber",
                schema: "inquiry",
                table: "BatchTravelers",
                column: "PassportNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_AgencyId",
                schema: "inquiry",
                table: "Inquiries",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_BatchId",
                schema: "inquiry",
                table: "Inquiries",
                column: "BatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_NationalityCode",
                schema: "inquiry",
                table: "Inquiries",
                column: "NationalityCode");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_PassportNumber",
                schema: "inquiry",
                table: "Inquiries",
                column: "PassportNumber");

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_ReferenceNumber",
                schema: "inquiry",
                table: "Inquiries",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inquiries_Status",
                schema: "inquiry",
                table: "Inquiries",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BatchTravelers",
                schema: "inquiry");

            migrationBuilder.DropTable(
                name: "Batches",
                schema: "inquiry");

            migrationBuilder.DropTable(
                name: "Inquiries",
                schema: "inquiry");
        }
    }
}
