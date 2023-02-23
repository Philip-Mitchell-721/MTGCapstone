using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTGCapstone.API.Migrations
{
    public partial class fixingDeckCardsAndCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeckCards_DeckCategories_DeckCategoryId",
                table: "DeckCards");

            migrationBuilder.DropIndex(
                name: "IX_DeckCards_DeckCategoryId",
                table: "DeckCards");

            migrationBuilder.DropColumn(
                name: "Board",
                table: "DeckCards");

            migrationBuilder.RenameColumn(
                name: "DeckCategoryId",
                table: "DeckCards",
                newName: "SideboardQuantity");

            migrationBuilder.AddColumn<int>(
                name: "DeckId",
                table: "DeckCards",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeckCategoryDeckCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeckCategoryId = table.Column<int>(type: "int", nullable: true),
                    DeckCardId = table.Column<int>(type: "int", nullable: true),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeckCategoryDeckCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeckCategoryDeckCards_DeckCards_DeckCardId",
                        column: x => x.DeckCardId,
                        principalTable: "DeckCards",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DeckCategoryDeckCards_DeckCategories_DeckCategoryId",
                        column: x => x.DeckCategoryId,
                        principalTable: "DeckCategories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeckCards_DeckId",
                table: "DeckCards",
                column: "DeckId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCategoryDeckCards_DeckCardId",
                table: "DeckCategoryDeckCards",
                column: "DeckCardId");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCategoryDeckCards_DeckCategoryId",
                table: "DeckCategoryDeckCards",
                column: "DeckCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCards_Decks_DeckId",
                table: "DeckCards",
                column: "DeckId",
                principalTable: "Decks",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DeckCards_Decks_DeckId",
                table: "DeckCards");

            migrationBuilder.DropTable(
                name: "DeckCategoryDeckCards");

            migrationBuilder.DropIndex(
                name: "IX_DeckCards_DeckId",
                table: "DeckCards");

            migrationBuilder.DropColumn(
                name: "DeckId",
                table: "DeckCards");

            migrationBuilder.RenameColumn(
                name: "SideboardQuantity",
                table: "DeckCards",
                newName: "DeckCategoryId");

            migrationBuilder.AddColumn<string>(
                name: "Board",
                table: "DeckCards",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_DeckCards_DeckCategoryId",
                table: "DeckCards",
                column: "DeckCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_DeckCards_DeckCategories_DeckCategoryId",
                table: "DeckCards",
                column: "DeckCategoryId",
                principalTable: "DeckCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
