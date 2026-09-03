using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace src.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaReservas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nombre_usuario = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nombre = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    apellido = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    password = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatar = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rol = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reserva",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_inmueble = table.Column<int>(type: "int", nullable: false),
                    id_inquilino = table.Column<int>(type: "int", nullable: false),
                    fecha_desde = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fecha_hasta = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    monto_diario = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    creado_por_user_id = table.Column<int>(type: "int", nullable: false),
                    terminado_por_user_id = table.Column<int>(type: "int", nullable: true),
                    activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reserva", x => x.id);
                    table.ForeignKey(
                        name: "FK_Reserva_Inmuebles_id_inmueble",
                        column: x => x.id_inmueble,
                        principalTable: "Inmuebles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reserva_Inquilino_id_inquilino",
                        column: x => x.id_inquilino,
                        principalTable: "Inquilino",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reserva_Usuario_creado_por_user_id",
                        column: x => x.creado_por_user_id,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reserva_Usuario_terminado_por_user_id",
                        column: x => x.terminado_por_user_id,
                        principalTable: "Usuario",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Inquilino",
                keyColumn: "id",
                keyValue: 3,
                column: "nombre",
                value: "Lucia");

            migrationBuilder.UpdateData(
                table: "Propietario",
                keyColumn: "id",
                keyValue: 1,
                column: "apellido",
                value: "Fernandez");

            migrationBuilder.UpdateData(
                table: "Propietario",
                keyColumn: "id",
                keyValue: 2,
                column: "apellido",
                value: "Lopez");

            migrationBuilder.UpdateData(
                table: "Propietario",
                keyColumn: "id",
                keyValue: 3,
                column: "apellido",
                value: "Garcia");

            migrationBuilder.InsertData(
                table: "Usuario",
                columns: new[] { "id", "activo", "apellido", "avatar", "email", "nombre", "nombre_usuario", "password", "rol" },
                values: new object[] { 1, true, "Sistema", null, "admin@gmail.com", "Administrador", "admin", "123456", "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_creado_por_user_id",
                table: "Reserva",
                column: "creado_por_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_id_inmueble",
                table: "Reserva",
                column: "id_inmueble");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_id_inquilino",
                table: "Reserva",
                column: "id_inquilino");

            migrationBuilder.CreateIndex(
                name: "IX_Reserva_terminado_por_user_id",
                table: "Reserva",
                column: "terminado_por_user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reserva");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.UpdateData(
                table: "Inquilino",
                keyColumn: "id",
                keyValue: 3,
                column: "nombre",
                value: "Lucía");

            migrationBuilder.UpdateData(
                table: "Propietario",
                keyColumn: "id",
                keyValue: 1,
                column: "apellido",
                value: "Fernández");

            migrationBuilder.UpdateData(
                table: "Propietario",
                keyColumn: "id",
                keyValue: 2,
                column: "apellido",
                value: "López");

            migrationBuilder.UpdateData(
                table: "Propietario",
                keyColumn: "id",
                keyValue: 3,
                column: "apellido",
                value: "García");
        }
    }
}
