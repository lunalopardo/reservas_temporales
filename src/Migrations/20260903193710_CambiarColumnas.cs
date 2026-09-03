using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace src.Migrations
{
    /// <inheritdoc />
    public partial class CambiarColumnas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inmueble_Propietario_IdPropietario",
                table: "Inmueble");

            migrationBuilder.RenameColumn(
                name: "Tipo",
                table: "Inmueble",
                newName: "tipo");

            migrationBuilder.RenameColumn(
                name: "Precio",
                table: "Inmueble",
                newName: "precio");

            migrationBuilder.RenameColumn(
                name: "Fotos",
                table: "Inmueble",
                newName: "fotos");

            migrationBuilder.RenameColumn(
                name: "Direccion",
                table: "Inmueble",
                newName: "direccion");

            migrationBuilder.RenameColumn(
                name: "Cupo",
                table: "Inmueble",
                newName: "cupo");

            migrationBuilder.RenameColumn(
                name: "Coord",
                table: "Inmueble",
                newName: "coord");

            migrationBuilder.RenameColumn(
                name: "Activo",
                table: "Inmueble",
                newName: "activo");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Inmueble",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "IdPropietario",
                table: "Inmueble",
                newName: "id_propietario");

            migrationBuilder.RenameColumn(
                name: "FotoPortada",
                table: "Inmueble",
                newName: "foto_portada");

            migrationBuilder.RenameIndex(
                name: "IX_Inmueble_IdPropietario",
                table: "Inmueble",
                newName: "IX_Inmueble_id_propietario");

            migrationBuilder.AlterColumn<decimal>(
                name: "precio",
                table: "Inmueble",
                type: "decimal(12,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_Inmueble_Propietario_id_propietario",
                table: "Inmueble",
                column: "id_propietario",
                principalTable: "Propietario",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inmueble_Propietario_id_propietario",
                table: "Inmueble");

            migrationBuilder.RenameColumn(
                name: "tipo",
                table: "Inmueble",
                newName: "Tipo");

            migrationBuilder.RenameColumn(
                name: "precio",
                table: "Inmueble",
                newName: "Precio");

            migrationBuilder.RenameColumn(
                name: "fotos",
                table: "Inmueble",
                newName: "Fotos");

            migrationBuilder.RenameColumn(
                name: "direccion",
                table: "Inmueble",
                newName: "Direccion");

            migrationBuilder.RenameColumn(
                name: "cupo",
                table: "Inmueble",
                newName: "Cupo");

            migrationBuilder.RenameColumn(
                name: "coord",
                table: "Inmueble",
                newName: "Coord");

            migrationBuilder.RenameColumn(
                name: "activo",
                table: "Inmueble",
                newName: "Activo");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Inmueble",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "id_propietario",
                table: "Inmueble",
                newName: "IdPropietario");

            migrationBuilder.RenameColumn(
                name: "foto_portada",
                table: "Inmueble",
                newName: "FotoPortada");

            migrationBuilder.RenameIndex(
                name: "IX_Inmueble_id_propietario",
                table: "Inmueble",
                newName: "IX_Inmueble_IdPropietario");

            migrationBuilder.AlterColumn<decimal>(
                name: "Precio",
                table: "Inmueble",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(12,2)");

            migrationBuilder.AddForeignKey(
                name: "FK_Inmueble_Propietario_IdPropietario",
                table: "Inmueble",
                column: "IdPropietario",
                principalTable: "Propietario",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
