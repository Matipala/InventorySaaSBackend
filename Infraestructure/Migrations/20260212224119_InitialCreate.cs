using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventorySaaS_Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    id_empresa = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.id_empresa);
                });

            migrationBuilder.CreateTable(
                name: "Almacenes",
                columns: table => new
                {
                    id_almacen = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Almacenes", x => x.id_almacen);
                    table.ForeignKey(
                        name: "FK_Almacenes_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.id_categoria);
                    table.ForeignKey(
                        name: "FK_Categorias_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documentos",
                columns: table => new
                {
                    id_documento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "BORRADOR"),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                name: "Productos",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.id_producto);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalTable: "Categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_Empresas_id_empresa",
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
                    id_documento = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    id_almacen = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Movimientos",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    id_documento = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    id_almacen = table.Column<int>(type: "integer", nullable: false),
                    tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    cantidad = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimientos", x => x.id_movimiento);
                    table.ForeignKey(
                        name: "FK_Movimientos_Almacenes_id_almacen",
                        column: x => x.id_almacen,
                        principalTable: "Almacenes",
                        principalColumn: "id_almacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimientos_Documentos_id_documento",
                        column: x => x.id_documento,
                        principalTable: "Documentos",
                        principalColumn: "id_documento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimientos_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimientos_Productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "Productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stock",
                columns: table => new
                {
                    id_stock = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    id_almacen = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stock", x => x.id_stock);
                    table.CheckConstraint("CK_Stock_Cantidad", "cantidad >= 0");
                    table.ForeignKey(
                        name: "FK_Stock_Almacenes_id_almacen",
                        column: x => x.id_almacen,
                        principalTable: "Almacenes",
                        principalColumn: "id_almacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stock_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stock_Productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "Productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Almacenes_id_empresa",
                table: "Almacenes",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_id_empresa_nombre",
                table: "Categorias",
                columns: new[] { "id_empresa", "nombre" },
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_nombre",
                table: "Empresas",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_fecha",
                table: "Movimientos",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_id_almacen",
                table: "Movimientos",
                column: "id_almacen");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_id_documento",
                table: "Movimientos",
                column: "id_documento");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_id_empresa",
                table: "Movimientos",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_id_producto",
                table: "Movimientos",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_tipo",
                table: "Movimientos",
                column: "tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_activo",
                table: "Productos",
                column: "activo");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_categoria",
                table: "Productos",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_empresa_sku",
                table: "Productos",
                columns: new[] { "id_empresa", "sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Stock_id_almacen",
                table: "Stock",
                column: "id_almacen");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_id_empresa",
                table: "Stock",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_id_producto",
                table: "Stock",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_id_producto_id_almacen",
                table: "Stock",
                columns: new[] { "id_producto", "id_almacen" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documento_detalle");

            migrationBuilder.DropTable(
                name: "Movimientos");

            migrationBuilder.DropTable(
                name: "Stock");

            migrationBuilder.DropTable(
                name: "Documentos");

            migrationBuilder.DropTable(
                name: "Almacenes");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Categorias");

            migrationBuilder.DropTable(
                name: "Empresas");
        }
    }
}
