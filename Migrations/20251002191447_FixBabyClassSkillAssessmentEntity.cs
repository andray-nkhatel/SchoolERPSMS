using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BluebirdCore.Migrations
{
    /// <inheritdoc />
    public partial class FixBabyClassSkillAssessmentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_SkillItemId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_Students_StudentId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedByTeacherId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.AddColumn<int>(
                name: "BabyClassSkillItemId",
                table: "BabyClassSkillAssessments",
                type: "int",
                nullable: true);

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
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_SkillItemId",
                table: "BabyClassSkillAssessments",
                column: "SkillItemId",
                principalTable: "BabyClassSkillItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_Students_StudentId",
                table: "BabyClassSkillAssessments",
                column: "StudentId",
                principalTable: "Students",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedByTeacherId",
                table: "BabyClassSkillAssessments",
                column: "AssessedByTeacherId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_BabyClassSkillItemId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_SkillItemId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_Students_StudentId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedByTeacherId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropIndex(
                name: "IX_BabyClassSkillAssessments_BabyClassSkillItemId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropColumn(
                name: "BabyClassSkillItemId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkillItems_SkillItemId",
                table: "BabyClassSkillAssessments",
                column: "SkillItemId",
                principalTable: "BabyClassSkillItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_Students_StudentId",
                table: "BabyClassSkillAssessments",
                column: "StudentId",
                principalTable: "Students",
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
    }
}
