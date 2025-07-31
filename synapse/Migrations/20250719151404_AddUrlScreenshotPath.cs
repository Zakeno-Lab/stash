using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace synapse.Migrations
{
    /// <inheritdoc />
    public partial class AddUrlScreenshotPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlScreenshotPath",
                table: "ClipboardItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrlScreenshotPath",
                table: "ClipboardItems");
        }
    }
}
