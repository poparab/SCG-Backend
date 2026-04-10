using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SCG.AgencyManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletTransactionPaymentMethodAndEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EvidenceFileName",
                schema: "agency",
                table: "WalletTransactions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                schema: "agency",
                table: "WalletTransactions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvidenceFileName",
                schema: "agency",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "agency",
                table: "WalletTransactions");
        }
    }
}
