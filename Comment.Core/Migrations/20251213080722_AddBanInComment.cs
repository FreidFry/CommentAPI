using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comment.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddBanInComment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBaned",
                table: "Comments",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBaned",
                table: "Comments");
        }
    }
}
