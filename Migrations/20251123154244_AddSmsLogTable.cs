using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BluebirdCore.Migrations
{
    /// <inheritdoc />
    public partial class AddSmsLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SmsLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MessageContent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: true),
                    SentByUserId = table.Column<int>(type: "int", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ProviderResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    MessageType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Term = table.Column<int>(type: "int", nullable: true),
                    AcademicYear = table.Column<int>(type: "int", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SmsLogs_AcademicYears_AcademicYear",
                        column: x => x.AcademicYear,
                        principalTable: "AcademicYears",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SmsLogs_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SmsLogs_Users_SentByUserId",
                        column: x => x.SentByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_AcademicYear",
                table: "SmsLogs",
                column: "AcademicYear");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_PhoneNumber",
                table: "SmsLogs",
                column: "PhoneNumber");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_SentAt",
                table: "SmsLogs",
                column: "SentAt");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_SentByUserId",
                table: "SmsLogs",
                column: "SentByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_Status",
                table: "SmsLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SmsLogs_StudentId",
                table: "SmsLogs",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SmsLogs");
        }
    }
}
