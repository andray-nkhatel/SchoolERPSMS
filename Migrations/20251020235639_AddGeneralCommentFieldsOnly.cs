using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BluebirdCore.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneralCommentFieldsOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeneralComment",
                table: "ReportCards",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "GeneralCommentUpdatedAt",
                table: "ReportCards",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GeneralCommentUpdatedBy",
                table: "ReportCards",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GeneralCommentUpdatedByUserId",
                table: "ReportCards",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReportCards_GeneralCommentUpdatedByUserId",
                table: "ReportCards",
                column: "GeneralCommentUpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportCards_Users_GeneralCommentUpdatedByUserId",
                table: "ReportCards",
                column: "GeneralCommentUpdatedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportCards_Users_GeneralCommentUpdatedByUserId",
                table: "ReportCards");

            migrationBuilder.DropIndex(
                name: "IX_ReportCards_GeneralCommentUpdatedByUserId",
                table: "ReportCards");

            migrationBuilder.DropColumn(
                name: "GeneralComment",
                table: "ReportCards");

            migrationBuilder.DropColumn(
                name: "GeneralCommentUpdatedAt",
                table: "ReportCards");

            migrationBuilder.DropColumn(
                name: "GeneralCommentUpdatedBy",
                table: "ReportCards");

            migrationBuilder.DropColumn(
                name: "GeneralCommentUpdatedByUserId",
                table: "ReportCards");
        }
    }
}
