using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comment.Core.Migrations
{
    /// <inheritdoc />
    public partial class addthreadincomment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Threads_ThreadModelId",
                table: "Comments");

            migrationBuilder.DropIndex(
                name: "IX_Comments_ThreadModelId",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "ThreadModelId",
                table: "Comments");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Threads_ThreadId",
                table: "Comments",
                column: "ThreadId",
                principalTable: "Threads",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Threads_ThreadId",
                table: "Comments");

            migrationBuilder.AddColumn<Guid>(
                name: "ThreadModelId",
                table: "Comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ThreadModelId",
                table: "Comments",
                column: "ThreadModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Threads_ThreadModelId",
                table: "Comments",
                column: "ThreadModelId",
                principalTable: "Threads",
                principalColumn: "Id");
        }
    }
}
