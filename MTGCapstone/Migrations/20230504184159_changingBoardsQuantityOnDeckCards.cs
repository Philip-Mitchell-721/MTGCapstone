using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTGCapstone.API.Migrations
{
    public partial class changingBoardsQuantityOnDeckCards : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "DeckCategoryDeckCards");

            migrationBuilder.AddColumn<string>(
                name: "Board",
                table: "DeckCards",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "DeckCards",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Board",
                table: "DeckCards");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "DeckCards");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "DeckCategoryDeckCards",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
