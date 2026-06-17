using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutsalReservation.Api.Migrations
{
    /// <inheritdoc />
    public partial class TambahBuktiTransfer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BuktiTransfer",
                table: "Pembayaran",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuktiTransfer",
                table: "Pembayaran");
        }
    }
}
