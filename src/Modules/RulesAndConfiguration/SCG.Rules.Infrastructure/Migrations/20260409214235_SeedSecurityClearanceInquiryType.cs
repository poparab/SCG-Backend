using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCG.Rules.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedSecurityClearanceInquiryType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                schema: "rules",
                table: "InquiryTypes",
                columns: new[] { "Id", "CreatedAt", "DescriptionAr", "DescriptionEn", "IsActive", "NameAr", "NameEn", "UpdatedAt" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000001"), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "طلب تصريح أمني للمسافرين", "Security clearance inquiry for travelers", true, "تصريح أمني", "Security Clearance", null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                schema: "rules",
                table: "InquiryTypes",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"));
        }
    }
}
