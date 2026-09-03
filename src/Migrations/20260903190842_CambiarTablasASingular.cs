using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace src.Migrations
{
    /// <inheritdoc />
    public partial class CambiarTablasASingular : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inmuebles_Propietario_IdPropietario",
                table: "Inmuebles");

            migrationBuilder.DropForeignKey(
                name: "FK_Reserva_Inmuebles_id_inmueble",
                table: "Reserva");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inmuebles",
                table: "Inmuebles");

            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Inmuebles");

            migrationBuilder.RenameTable(
                name: "Inmuebles",
                newName: "Inmueble");

            migrationBuilder.RenameIndex(
                name: "IX_Inmuebles_IdPropietario",
                table: "Inmueble",
                newName: "IX_Inmueble_IdPropietario");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inmueble",
                table: "Inmueble",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Inmueble_Propietario_IdPropietario",
                table: "Inmueble",
                column: "IdPropietario",
                principalTable: "Propietario",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reserva_Inmueble_id_inmueble",
                table: "Reserva",
                column: "id_inmueble",
                principalTable: "Inmueble",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inmueble_Propietario_IdPropietario",
                table: "Inmueble");

            migrationBuilder.DropForeignKey(
                name: "FK_Reserva_Inmueble_id_inmueble",
                table: "Reserva");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Inmueble",
                table: "Inmueble");

            migrationBuilder.RenameTable(
                name: "Inmueble",
                newName: "Inmuebles");

            migrationBuilder.RenameIndex(
                name: "IX_Inmueble_IdPropietario",
                table: "Inmuebles",
                newName: "IX_Inmuebles_IdPropietario");

            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Inmuebles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inmuebles",
                table: "Inmuebles",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "Inmuebles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Estado",
                value: true);

            migrationBuilder.UpdateData(
                table: "Inmuebles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Estado",
                value: true);

            migrationBuilder.UpdateData(
                table: "Inmuebles",
                keyColumn: "Id",
                keyValue: 3,
                column: "Estado",
                value: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Inmuebles_Propietario_IdPropietario",
                table: "Inmuebles",
                column: "IdPropietario",
                principalTable: "Propietario",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Reserva_Inmuebles_id_inmueble",
                table: "Reserva",
                column: "id_inmueble",
                principalTable: "Inmuebles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
