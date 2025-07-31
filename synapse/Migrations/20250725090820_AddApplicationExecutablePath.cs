using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace synapse.Migrations
{
    /// <inheritdoc />
    public partial class AddApplicationExecutablePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationExecutablePath",
                table: "ClipboardItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationExecutablePath",
                table: "ClipboardItems");
        }
    }
}
