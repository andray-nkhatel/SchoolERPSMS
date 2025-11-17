using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BluebirdCore.Migrations
{
    /// <inheritdoc />
    public partial class AddBabyClassSkillAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BabyClassSkills",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BabyClassSkills", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BabyClassSkillItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SkillId = table.Column<int>(type: "int", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BabyClassSkillItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BabyClassSkillItems_BabyClassSkills_SkillId",
                        column: x => x.SkillId,
                        principalTable: "BabyClassSkills",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BabyClassSkillAssessments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SkillItemId = table.Column<int>(type: "int", nullable: false),
                    AcademicYear = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    TeacherComment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AssessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssessedBy = table.Column<int>(type: "int", nullable: false),
                    BabyClassSkillId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BabyClassSkillAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_SkillItemId",
                        column: x => x.SkillItemId,
                        principalTable: "BabyClassSkillItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BabyClassSkillAssessments_BabyClassSkills_BabyClassSkillId",
                        column: x => x.BabyClassSkillId,
                        principalTable: "BabyClassSkills",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BabyClassSkillAssessments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BabyClassSkillAssessments_Users_AssessedBy",
                        column: x => x.AssessedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "BabyClassSkills",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Order" },
                values: new object[,]
                {
                    { 1, "Verbal communication abilities", true, "Communication Skills", 1 },
                    { 2, "Social interaction and emotional development", true, "Social Emotional Skills", 2 },
                    { 3, "Early literacy skills", true, "Reading & Writing", 3 },
                    { 4, "Visual recognition and identification", true, "Colour & Shapes", 4 },
                    { 5, "Basic numeracy skills", true, "Numbers", 5 },
                    { 6, "Small muscle coordination and control", true, "Fine-Motor Skills", 6 },
                    { 7, "Large muscle movement and coordination", true, "Gross Motor Skills", 7 }
                });

            migrationBuilder.InsertData(
                table: "BabyClassSkillItems",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Order", "SkillId" },
                values: new object[,]
                {
                    { 1, "Ability to articulate words clearly", true, "Speaks Clearly", 1, 1 },
                    { 2, "Ability to answer questions appropriately", true, "Responds to direct questions", 2, 1 },
                    { 3, "Recognition and response to own name", true, "Know first name", 1, 2 },
                    { 4, "Ability to follow simple instructions", true, "Follows Instruction", 2, 2 },
                    { 5, "Cooperative play and sharing behavior", true, "Shares well with others", 3, 2 },
                    { 6, "Recognition and pronunciation of letter sounds", true, "Know how to say letterland characters", 1, 3 },
                    { 7, "Phonetic awareness and sound production", true, "Able to say sounds", 2, 3 },
                    { 8, "Recognition of basic colors", true, "Know Primary Colours", 1, 4 },
                    { 9, "Recognition of basic geometric shapes", true, "Knows Shapes", 2, 4 },
                    { 10, "Basic counting ability", true, "Able to count", 1, 5 },
                    { 11, "Verbal counting from 1 to 10", true, "Orally from 1 - 10", 2, 5 },
                    { 12, "Pencil grip and control", true, "Can hold and use a pencil", 1, 6 },
                    { 13, "Crayon grip and coloring ability", true, "Can hold and use a Crayon", 2, 6 },
                    { 14, "Tracing and copying skills", true, "Able to Trace", 3, 6 },
                    { 15, "Basic jumping and movement coordination", true, "Can jump up and down", 1, 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_AssessedBy",
                table: "BabyClassSkillAssessments",
                column: "AssessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_BabyClassSkillId",
                table: "BabyClassSkillAssessments",
                column: "BabyClassSkillId");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_SkillItemId",
                table: "BabyClassSkillAssessments",
                column: "SkillItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_StudentId_SkillItemId_AcademicYear_Term",
                table: "BabyClassSkillAssessments",
                columns: new[] { "StudentId", "SkillItemId", "AcademicYear", "Term" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillItems_SkillId",
                table: "BabyClassSkillItems",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkills_Name",
                table: "BabyClassSkills",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BabyClassSkillAssessments");

            migrationBuilder.DropTable(
                name: "BabyClassSkillItems");

            migrationBuilder.DropTable(
                name: "BabyClassSkills");
        }
    }
}
