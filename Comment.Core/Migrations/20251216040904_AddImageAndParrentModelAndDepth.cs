using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comment.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddImageAndParrentModelAndDepth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommentModelId",
                table: "Comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentDepth",
                table: "Comments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CommentModelId",
                table: "Comments",
                column: "CommentModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_CommentModelId",
                table: "Comments",
                column: "CommentModelId",
                principalTable: "Comments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_CommentModelId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_CommentModelId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "CommentModelId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ParentDepth",
                table: "Comments");
        }
    }
}
