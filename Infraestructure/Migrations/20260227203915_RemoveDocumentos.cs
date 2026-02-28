using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventorySaaS_Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Movimientos_Documentos_id_documento",
                table: "Movimientos");

            migrationBuilder.DropTable(
                name: "documento_detalle");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropIndex(
                name: "IX_Movimientos_id_documento",
                table: "Movimientos");

            migrationBuilder.DropColumn(
                name: "id_documento",
                table: "Movimientos");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_documento",
                table: "Movimientos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    id_documento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "BORRADOR"),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentos", x => x.id_documento);
                    table.CheckConstraint("CK_Documentos_Estado", "estado IN ('BORRADOR', 'CONFIRMADO')");
                    table.ForeignKey(
                        name: "FK_Documentos_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "documento_detalle",
                columns: table => new
                {
                    id_detalle = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    id_almacen = table.Column<int>(type: "integer", nullable: false),
                    id_documento = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documento_detalle", x => x.id_detalle);
                    table.CheckConstraint("CK_DocumentoDetalle_Cantidad", "cantidad > 0");
                    table.ForeignKey(
                        name: "FK_documento_detalle_Almacenes_id_almacen",
                        column: x => x.id_almacen,
                        principalTable: "Almacenes",
                        principalColumn: "id_almacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_documento_detalle_Documentos_id_documento",
                        column: x => x.id_documento,
                        principalTable: "Documentos",
                        principalColumn: "id_documento",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_documento_detalle_Productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "Productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_id_documento",
                table: "Movimientos",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_documento_detalle_id_almacen",
                table: "documento_detalle",
                column: "id_almacen");

            migrationBuilder.CreateIndex(
                name: "IX_documento_detalle_id_documento",
                table: "documento_detalle",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_documento_detalle_id_producto",
                table: "documento_detalle",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_estado",
                table: "Documentos",
                column: "estado");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_fecha",
                table: "Documentos",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_id_empresa",
                table: "Documentos",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_Documentos_tipo",
                table: "Documentos",
                column: "tipo");

            migrationBuilder.AddForeignKey(
                name: "FK_Movimientos_Documentos_id_documento",
                table: "Movimientos",
                column: "id_documento",
                principalTable: "Documentos",
                principalColumn: "id_documento",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
