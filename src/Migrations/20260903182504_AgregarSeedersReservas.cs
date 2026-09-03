using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace src.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSeedersReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Reserva",
                columns: new[] { "id", "activo", "creado_por_user_id", "fecha_desde", "fecha_hasta", "id_inmueble", "id_inquilino", "monto_diario", "terminado_por_user_id" },
                values: new object[,]
                {
                    { 1, true, 1, new DateTime(2026, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 9, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, 45000.00m, null },
                    { 2, true, 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 10, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, 75000.00m, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Reserva",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Reserva",
                keyColumn: "id",
                keyValue: 2);
        }
    }
}
