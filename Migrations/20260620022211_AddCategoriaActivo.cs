using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace inventorysaasbackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriaActivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "activo",
                schema: "inventario",
                table: "Categorias",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "activo",
                schema: "inventario",
                table: "Categorias");
        }
    }
}
