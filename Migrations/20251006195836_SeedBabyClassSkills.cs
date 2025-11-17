using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BluebirdCore.Migrations
{
    /// <inheritdoc />
    public partial class SeedBabyClassSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_BabyClassSkillItemId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedByTeacherId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropIndex(
                name: "IX_BabyClassSkillAssessments_AssessedByTeacherId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropIndex(
                name: "IX_BabyClassSkillAssessments_BabyClassSkillItemId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropColumn(
                name: "AssessedByTeacherId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropColumn(
                name: "BabyClassSkillItemId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.InsertData(
                table: "BabyClassSkills",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Order" },
                values: new object[,]
                {
                    { 1, "Language and communication development", true, "Communication Skills", 1 },
                    { 2, "Hand-eye coordination and dexterity", true, "Fine Motor Skills", 2 },
                    { 3, "Large muscle movement and coordination", true, "Gross Motor Skills", 3 },
                    { 4, "Social interaction and emotional development", true, "Social-Emotional Skills", 4 },
                    { 5, "Thinking and problem-solving abilities", true, "Cognitive Skills", 5 }
                });

            migrationBuilder.InsertData(
                table: "BabyClassSkillItems",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Order", "SkillId" },
                values: new object[,]
                {
                    { 1, "Can articulate words clearly", true, "Speaks clearly", 1, 1 },
                    { 2, "Pays attention when spoken to", true, "Listens attentively", 2, 1 },
                    { 3, "Can follow simple 2-3 step instructions", true, "Follows instructions", 3, 1 },
                    { 4, "Proper pencil grip", true, "Holds pencil correctly", 1, 2 },
                    { 5, "Can use safety scissors", true, "Cuts with scissors", 2, 2 },
                    { 6, "Can draw circles, squares, triangles", true, "Draws basic shapes", 3, 2 },
                    { 7, "Can run without falling", true, "Runs confidently", 1, 3 },
                    { 8, "Can jump forward and backward", true, "Jumps with both feet", 2, 3 },
                    { 9, "Can balance for 3-5 seconds", true, "Balances on one foot", 3, 3 },
                    { 10, "Willingly shares toys and materials", true, "Shares with others", 1, 4 },
                    { 11, "Waits for turn in group activities", true, "Takes turns", 2, 4 },
                    { 12, "Shows concern for others' feelings", true, "Shows empathy", 3, 4 },
                    { 13, "Can identify primary colors", true, "Recognizes colors", 1, 5 },
                    { 14, "Can count objects up to 10", true, "Counts to 10", 2, 5 },
                    { 15, "Can group objects by attributes", true, "Sorts objects", 3, 5 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkills_Name",
                table: "BabyClassSkills",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BabyClassSkills_Name",
                table: "BabyClassSkills");

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

            migrationBuilder.AddColumn<int>(
                name: "AssessedByTeacherId",
                table: "BabyClassSkillAssessments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BabyClassSkillItemId",
                table: "BabyClassSkillAssessments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_AssessedByTeacherId",
                table: "BabyClassSkillAssessments",
                column: "AssessedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_BabyClassSkillItemId",
                table: "BabyClassSkillAssessments",
                column: "BabyClassSkillItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_BabyClassSkillItemId",
                table: "BabyClassSkillAssessments",
                column: "BabyClassSkillItemId",
                principalTable: "BabyClassSkillItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedByTeacherId",
                table: "BabyClassSkillAssessments",
                column: "AssessedByTeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
