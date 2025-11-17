using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BluebirdCore.Migrations
{
    /// <inheritdoc />
    public partial class AddBabyClassSkillTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_SkillItemId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedBy",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropIndex(
                name: "IX_BabyClassSkills_Name",
                table: "BabyClassSkills");

            migrationBuilder.DropIndex(
                name: "IX_BabyClassSkillAssessments_AssessedBy",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropIndex(
                name: "IX_BabyClassSkillAssessments_StudentId_SkillItemId_AcademicYear_Term",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "BabyClassSkillItems",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "BabyClassSkills",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "BabyClassSkills",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "BabyClassSkills",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "BabyClassSkills",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "BabyClassSkills",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "BabyClassSkills",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "BabyClassSkills",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.AddColumn<int>(
                name: "AssessedByTeacherId",
                table: "BabyClassSkillAssessments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_AssessedByTeacherId",
                table: "BabyClassSkillAssessments",
                column: "AssessedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_StudentId",
                table: "BabyClassSkillAssessments",
                column: "StudentId");

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_SkillItemId",
                table: "BabyClassSkillAssessments",
                column: "SkillItemId",
                principalTable: "BabyClassSkillItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedByTeacherId",
                table: "BabyClassSkillAssessments",
                column: "AssessedByTeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_SkillItemId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedByTeacherId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropIndex(
                name: "IX_BabyClassSkillAssessments_AssessedByTeacherId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropIndex(
                name: "IX_BabyClassSkillAssessments_StudentId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropColumn(
                name: "AssessedByTeacherId",
                table: "BabyClassSkillAssessments");

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
                name: "IX_BabyClassSkills_Name",
                table: "BabyClassSkills",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_AssessedBy",
                table: "BabyClassSkillAssessments",
                column: "AssessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_StudentId_SkillItemId_AcademicYear_Term",
                table: "BabyClassSkillAssessments",
                columns: new[] { "StudentId", "SkillItemId", "AcademicYear", "Term" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_SkillItemId",
                table: "BabyClassSkillAssessments",
                column: "SkillItemId",
                principalTable: "BabyClassSkillItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedBy",
                table: "BabyClassSkillAssessments",
                column: "AssessedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
