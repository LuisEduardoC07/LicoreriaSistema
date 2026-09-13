using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicoreriaSistema.Datos.Migrations
{
    /// <inheritdoc />
    public partial class AlcanceGlobalUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlcanceGlobal",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlcanceGlobal",
                table: "Usuarios");
        }
    }
}
