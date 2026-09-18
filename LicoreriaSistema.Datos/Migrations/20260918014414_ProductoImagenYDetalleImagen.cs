using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicoreriaSistema.Datos.Migrations
{
    /// <inheritdoc />
    public partial class ProductoImagenYDetalleImagen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "VentasDetalles",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Productos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "VentasDetalles");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Productos");
        }
    }
}
