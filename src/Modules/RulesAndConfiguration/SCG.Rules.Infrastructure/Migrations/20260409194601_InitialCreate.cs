using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCG.Rules.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rules");

            migrationBuilder.CreateTable(
                name: "AgencyCategories",
                schema: "rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InquiryTypes",
                schema: "rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DescriptionAr = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InquiryTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Nationalities",
                schema: "rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RequiresInquiry = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Nationalities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionWindows",
                schema: "rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OpenTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    CloseTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionWindows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemAnnouncements",
                schema: "rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TitleAr = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TitleEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MessageAr = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    MessageEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "info"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemAnnouncements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pricings",
                schema: "rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InquiryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NationalityCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Fee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pricings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pricings_InquiryTypes_InquiryTypeId",
                        column: x => x.InquiryTypeId,
                        principalSchema: "rules",
                        principalTable: "InquiryTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NationalityInquiryTypes",
                schema: "rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NationalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InquiryTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NationalityInquiryTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NationalityInquiryTypes_InquiryTypes_InquiryTypeId",
                        column: x => x.InquiryTypeId,
                        principalSchema: "rules",
                        principalTable: "InquiryTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NationalityInquiryTypes_Nationalities_NationalityId",
                        column: x => x.NationalityId,
                        principalSchema: "rules",
                        principalTable: "Nationalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Nationalities_Code",
                schema: "rules",
                table: "Nationalities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NationalityInquiryTypes_InquiryTypeId",
                schema: "rules",
                table: "NationalityInquiryTypes",
                column: "InquiryTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_NationalityInquiryTypes_NationalityId_InquiryTypeId",
                schema: "rules",
                table: "NationalityInquiryTypes",
                columns: new[] { "NationalityId", "InquiryTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pricings_InquiryTypeId_IsActive",
                schema: "rules",
                table: "Pricings",
                columns: new[] { "InquiryTypeId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionWindows_DayOfWeek_IsActive",
                schema: "rules",
                table: "SubmissionWindows",
                columns: new[] { "DayOfWeek", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyCategories",
                schema: "rules");

            migrationBuilder.DropTable(
                name: "NationalityInquiryTypes",
                schema: "rules");

            migrationBuilder.DropTable(
                name: "Pricings",
                schema: "rules");

            migrationBuilder.DropTable(
                name: "SubmissionWindows",
                schema: "rules");

            migrationBuilder.DropTable(
                name: "SystemAnnouncements",
                schema: "rules");

            migrationBuilder.DropTable(
                name: "Nationalities",
                schema: "rules");

            migrationBuilder.DropTable(
                name: "InquiryTypes",
                schema: "rules");
        }
    }
}
