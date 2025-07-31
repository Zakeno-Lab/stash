using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace synapse.Migrations
{
    /// <inheritdoc />
    public partial class AddClipboardItemMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CharacterCount",
                table: "ClipboardItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageHeight",
                table: "ClipboardItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageWidth",
                table: "ClipboardItems",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WindowTitle",
                table: "ClipboardItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WordCount",
                table: "ClipboardItems",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CharacterCount",
                table: "ClipboardItems");

            migrationBuilder.DropColumn(
                name: "ImageHeight",
                table: "ClipboardItems");

            migrationBuilder.DropColumn(
                name: "ImageWidth",
                table: "ClipboardItems");

            migrationBuilder.DropColumn(
                name: "WindowTitle",
                table: "ClipboardItems");

            migrationBuilder.DropColumn(
                name: "WordCount",
                table: "ClipboardItems");
        }
    }
}
