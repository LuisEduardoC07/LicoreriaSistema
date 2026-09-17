using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LicoreriaSistema.Datos.Migrations
{
    /// <inheritdoc />
    public partial class ActualizacionPermisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClasificacionFiscalId",
                table: "Productos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClasificacionesFiscales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasificacionesFiscales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReglasImpuesto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClasificacionFiscalId = table.Column<int>(type: "int", nullable: false),
                    TipoImpuesto = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TasaAdValorem = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    MontoEspecifico = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                    UnidadCalculo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasImpuesto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasImpuesto_ClasificacionesFiscales_ClasificacionFiscalId",
                        column: x => x.ClasificacionFiscalId,
                        principalTable: "ClasificacionesFiscales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_ClasificacionFiscalId",
                table: "Productos",
                column: "ClasificacionFiscalId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasificacionesFiscales_Nombre",
                table: "ClasificacionesFiscales",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReglasImpuesto_ClasificacionFiscalId_TipoImpuesto_FechaInicio",
                table: "ReglasImpuesto",
                columns: new[] { "ClasificacionFiscalId", "TipoImpuesto", "FechaInicio" });

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_ClasificacionesFiscales_ClasificacionFiscalId",
                table: "Productos",
                column: "ClasificacionFiscalId",
                principalTable: "ClasificacionesFiscales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_ClasificacionesFiscales_ClasificacionFiscalId",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "ReglasImpuesto");

            migrationBuilder.DropTable(
                name: "ClasificacionesFiscales");

            migrationBuilder.DropIndex(
                name: "IX_Productos_ClasificacionFiscalId",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "ClasificacionFiscalId",
                table: "Productos");
        }
    }
}
