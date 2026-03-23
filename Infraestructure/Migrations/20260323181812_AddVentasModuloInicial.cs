using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventorySaaS_Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddVentasModuloInicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "agotado_86",
                table: "Productos",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "id_unidad",
                table: "Productos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "precio_venta",
                table: "Productos",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Unidades",
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
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ventas_Clientes",
                columns: table => new
                {
                    id_cliente = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas_Clientes", x => x.id_cliente);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ventas_CuentasTickets",
                columns: table => new
                {
                    id_cuenta_ticket = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    id_almacen = table.Column<int>(type: "integer", nullable: false),
                    id_cliente = table.Column<int>(type: "integer", nullable: true),
                    numero = table.Column<int>(type: "integer", nullable: false),
                    mesero = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    estado = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    impuesto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    total = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas_CuentasTickets", x => x.id_cuenta_ticket);
                    table.ForeignKey(
                        name: "FK_Ventas_CuentasTickets_Almacenes_id_almacen",
                        column: x => x.id_almacen,
                        principalTable: "Almacenes",
                        principalColumn: "id_almacen",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_CuentasTickets_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_CuentasTickets_Ventas_Clientes_id_cliente",
                        column: x => x.id_cliente,
                        principalTable: "Ventas_Clientes",
                        principalColumn: "id_cliente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Ventas_CuentasTicketItems",
                columns: table => new
                {
                    id_cuenta_ticket_item = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cuenta_ticket = table.Column<int>(type: "integer", nullable: false),
                    id_producto = table.Column<int>(type: "integer", nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    precio_unitario = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    subtotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    nota = table.Column<string>(type: "text", nullable: true),
                    comanda_enviada = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas_CuentasTicketItems", x => x.id_cuenta_ticket_item);
                    table.ForeignKey(
                        name: "FK_Ventas_CuentasTicketItems_Productos_id_producto",
                        column: x => x.id_producto,
                        principalTable: "Productos",
                        principalColumn: "id_producto",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_CuentasTicketItems_Ventas_CuentasTickets_id_cuenta_t~",
                        column: x => x.id_cuenta_ticket,
                        principalTable: "Ventas_CuentasTickets",
                        principalColumn: "id_cuenta_ticket",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ventas_Pagos",
                columns: table => new
                {
                    id_pago = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_empresa = table.Column<int>(type: "integer", nullable: false),
                    id_cuenta_ticket = table.Column<int>(type: "integer", nullable: false),
                    metodo_pago = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    monto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas_Pagos", x => x.id_pago);
                    table.ForeignKey(
                        name: "FK_Ventas_Pagos_Empresas_id_empresa",
                        column: x => x.id_empresa,
                        principalTable: "Empresas",
                        principalColumn: "id_empresa",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Pagos_Ventas_CuentasTickets_id_cuenta_ticket",
                        column: x => x.id_cuenta_ticket,
                        principalTable: "Ventas_CuentasTickets",
                        principalColumn: "id_cuenta_ticket",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Productos_id_unidad",
                table: "Productos",
                column: "id_unidad");

            migrationBuilder.CreateIndex(
                name: "IX_Unidades_id_empresa_nombre",
                table: "Unidades",
                columns: new[] { "id_empresa", "nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Clientes_id_empresa",
                table: "Ventas_Clientes",
                column: "id_empresa");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Clientes_id_empresa_telefono",
                table: "Ventas_Clientes",
                columns: new[] { "id_empresa", "telefono" });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CuentasTicketItems_comanda_enviada",
                table: "Ventas_CuentasTicketItems",
                column: "comanda_enviada");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CuentasTicketItems_id_cuenta_ticket",
                table: "Ventas_CuentasTicketItems",
                column: "id_cuenta_ticket");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CuentasTicketItems_id_producto",
                table: "Ventas_CuentasTicketItems",
                column: "id_producto");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CuentasTickets_fecha_creacion",
                table: "Ventas_CuentasTickets",
                column: "fecha_creacion");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CuentasTickets_id_almacen",
                table: "Ventas_CuentasTickets",
                column: "id_almacen");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CuentasTickets_id_cliente",
                table: "Ventas_CuentasTickets",
                column: "id_cliente");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CuentasTickets_id_empresa_estado",
                table: "Ventas_CuentasTickets",
                columns: new[] { "id_empresa", "estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_CuentasTickets_id_empresa_numero",
                table: "Ventas_CuentasTickets",
                columns: new[] { "id_empresa", "numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Pagos_fecha_pago",
                table: "Ventas_Pagos",
                column: "fecha_pago");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Pagos_id_cuenta_ticket",
                table: "Ventas_Pagos",
                column: "id_cuenta_ticket");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Pagos_id_empresa",
                table: "Ventas_Pagos",
                column: "id_empresa");

            migrationBuilder.AddForeignKey(
                name: "FK_Productos_Unidades_id_unidad",
                table: "Productos",
                column: "id_unidad",
                principalTable: "Unidades",
                principalColumn: "id_unidad",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Productos_Unidades_id_unidad",
                table: "Productos");

            migrationBuilder.DropTable(
                name: "Unidades");

            migrationBuilder.DropTable(
                name: "Ventas_CuentasTicketItems");

            migrationBuilder.DropTable(
                name: "Ventas_Pagos");

            migrationBuilder.DropTable(
                name: "Ventas_CuentasTickets");

            migrationBuilder.DropTable(
                name: "Ventas_Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Productos_id_unidad",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "agotado_86",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "id_unidad",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "precio_venta",
                table: "Productos");
        }
    }
}
