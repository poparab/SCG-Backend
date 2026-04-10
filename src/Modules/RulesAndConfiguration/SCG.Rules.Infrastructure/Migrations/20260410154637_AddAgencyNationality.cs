using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCG.Rules.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyNationality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgencyNationalities",
                schema: "rules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NationalityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomFee = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyNationalities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgencyNationalities_Nationalities_NationalityId",
                        column: x => x.NationalityId,
                        principalSchema: "rules",
                        principalTable: "Nationalities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyNationalities_AgencyId",
                schema: "rules",
                table: "AgencyNationalities",
                column: "AgencyId");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyNationalities_AgencyId_NationalityId",
                schema: "rules",
                table: "AgencyNationalities",
                columns: new[] { "AgencyId", "NationalityId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgencyNationalities_NationalityId",
                schema: "rules",
                table: "AgencyNationalities",
                column: "NationalityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyNationalities",
                schema: "rules");
        }
    }
}
