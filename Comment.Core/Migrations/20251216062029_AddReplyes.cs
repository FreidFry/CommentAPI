using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comment.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddReplyes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "ImageTumbnailUrl",
                table: "Comments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Comments",
                type: "text",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments",
                column: "ParentCommentId",
                principalTable: "Comments",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Comments_ParentCommentId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ImageTumbnailUrl",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Comments");

            migrationBuilder.AddColumn<Guid>(
                name: "CommentModelId",
                table: "Comments",
                type: "uuid",
                nullable: true);

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
    }
}
