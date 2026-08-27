using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace src.Migrations
{
    /// <inheritdoc />
    public partial class InmueblesEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Estado",
                table: "Inmuebles",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Inmuebles");
        }
    }
}
