using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inventorysaasbackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "estacion",
                schema: "inventario",
                table: "Productos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo",
                schema: "inventario",
                table: "Movimientos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estacion",
                schema: "inventario",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "motivo",
                schema: "inventario",
                table: "Movimientos");
        }
    }
}
