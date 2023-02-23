using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTGCapstone.API.Migrations
{
    public partial class updatedDeckBoardsToCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "DeckCards");

            migrationBuilder.DropColumn(
                name: "SideboardQuantity",
                table: "DeckCards");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "DeckCards",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SideboardQuantity",
                table: "DeckCards",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
