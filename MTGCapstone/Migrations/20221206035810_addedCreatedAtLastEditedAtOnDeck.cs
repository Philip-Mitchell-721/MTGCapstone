using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MTGCapstone.API.Migrations
{
    public partial class addedCreatedAtLastEditedAtOnDeck : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Decks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEditedAt",
                table: "Decks",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Decks");

            migrationBuilder.DropColumn(
                name: "LastEditedAt",
                table: "Decks");
        }
    }
}
