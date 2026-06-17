using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutsalReservation.Api.Migrations
{
    /// <inheritdoc />
    public partial class TambahGambarLapangan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Galeri",
                table: "Lapangan",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GambarUtama",
                table: "Lapangan",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Galeri",
                table: "Lapangan");

            migrationBuilder.DropColumn(
                name: "GambarUtama",
                table: "Lapangan");
        }
    }
}
