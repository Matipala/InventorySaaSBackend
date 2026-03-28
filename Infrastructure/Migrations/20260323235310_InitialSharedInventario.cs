using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace inventorysaasbackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSharedInventario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventario");

            migrationBuilder.EnsureSchema(
                name: "shared");

            migrationBuilder.CreateTable(
                name: "Empresas",
                schema: "shared",
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
                schema: "inventario",
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
                        principalSchema: "shared",
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Categorias",
                schema: "inventario",
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
                        principalSchema: "shared",
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Unidades",
                schema: "inventario",
                columns: table => new
                {
                    id_unidad = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    abreviatura = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unidades", x => x.id_unidad);
                    table.ForeignKey(
                        name: "FK_Unidades_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalSchema: "shared",
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                schema: "inventario",
                columns: table => new
                {
                    id_producto = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    id_categoria = table.Column<int>(type: "integer", nullable: false),
                    id_unidad = table.Column<int>(type: "integer", nullable: true),
                    precio_venta = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    agotado_86 = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.id_producto);
                    table.ForeignKey(
                        name: "FK_Productos_Categorias_id_categoria",
                        column: x => x.id_categoria,
                        principalSchema: "inventario",
                        principalTable: "Categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalSchema: "shared",
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Productos_Unidades_id_unidad",
                        column: x => x.id_unidad,
                        principalSchema: "inventario",
                        principalTable: "Unidades",
                        principalColumn: "id_unidad",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Movimientos",
                schema: "inventario",
                columns: table => new
                {
                    id_movimiento = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
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
                        principalSchema: "inventario",
                        principalTable: "Almacenes",
                        principalColumn: "id_almacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimientos_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalSchema: "shared",
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Movimientos_Productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "inventario",
                        principalTable: "Productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Stock",
                schema: "inventario",
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
                        principalSchema: "inventario",
                        principalTable: "Almacenes",
                        principalColumn: "id_almacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stock_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalSchema: "shared",
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Stock_Productos_id_producto",
                        column: x => x.id_producto,
                        principalSchema: "inventario",
                        principalTable: "Productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Almacenes_id_empresa",
                schema: "inventario",
                table: "Almacenes",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_Categorias_id_empresa_nombre",
                schema: "inventario",
                table: "Categorias",
                columns: new[] { "id_empresa", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_nombre",
                schema: "shared",
                table: "Empresas",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_fecha",
                schema: "inventario",
                table: "Movimientos",
                column: "fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_id_almacen",
                schema: "inventario",
                table: "Movimientos",
                column: "id_almacen");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_id_empresa",
                schema: "inventario",
                table: "Movimientos",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_id_producto",
                schema: "inventario",
                table: "Movimientos",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_tipo",
                schema: "inventario",
                table: "Movimientos",
                column: "tipo");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_activo",
                schema: "inventario",
                table: "Productos",
                column: "activo");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_categoria",
                schema: "inventario",
                table: "Productos",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_empresa_sku",
                schema: "inventario",
                table: "Productos",
                columns: new[] { "id_empresa", "sku" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_unidad",
                schema: "inventario",
                table: "Productos",
                column: "id_unidad");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_id_almacen",
                schema: "inventario",
                table: "Stock",
                column: "id_almacen");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_id_empresa",
                schema: "inventario",
                table: "Stock",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_id_producto",
                schema: "inventario",
                table: "Stock",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_Stock_id_producto_id_almacen",
                schema: "inventario",
                table: "Stock",
                columns: new[] { "id_producto", "id_almacen" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_id_empresa_nombre",
                schema: "inventario",
                table: "Unidades",
                columns: new[] { "id_empresa", "nombre" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Movimientos",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "Stock",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "Almacenes",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "Productos",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "Categorias",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "Unidades",
                schema: "inventario");

            migrationBuilder.DropTable(
                name: "Empresas",
                schema: "shared");
        }
    }
}
