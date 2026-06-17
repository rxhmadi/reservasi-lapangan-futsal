using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutsalReservation.Api.Migrations
{
    /// <inheritdoc />
    public partial class MetodePembayaranDanKodeBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KodeBooking",
                table: "Pembayaran",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MetodePembayaran",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipe = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nama = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NomorAkun = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    AtasNama = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Instruksi = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    DibuatPada = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MetodePembayaran", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MetodePembayaran");

            migrationBuilder.DropColumn(
                name: "KodeBooking",
                table: "Pembayaran");
        }
    }
}
