using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileStoreBackend.Migrations
{
    /// <inheritdoc />
    public partial class add_state : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_done",
                table: "files",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_done",
                table: "files");
        }
    }
}
