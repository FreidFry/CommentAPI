using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comment.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddFileUrlInComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileUrl",
                table: "Comments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileUrl",
                table: "Comments");
        }
    }
}
