using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BluebirdCore.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkills_BabyClassSkillId",
                table: "BabyClassSkillAssessments");

            migrationBuilder.RenameColumn(
                name: "BabyClassSkillId",
                table: "BabyClassSkillAssessments",
                newName: "BabyClassSkillId1");

            migrationBuilder.RenameIndex(
                name: "IX_BabyClassSkillAssessments_BabyClassSkillId",
                table: "BabyClassSkillAssessments",
                newName: "IX_BabyClassSkillAssessments_BabyClassSkillId1");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_AssessedBy",
                table: "BabyClassSkillAssessments",
                column: "AssessedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkills_BabyClassSkillId1",
                table: "BabyClassSkillAssessments",
                column: "BabyClassSkillId1",
                principalTable: "BabyClassSkills",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedBy",
                table: "BabyClassSkillAssessments",
                column: "AssessedBy",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkills_BabyClassSkillId1",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropForeignKey(
                name: "FK_BabyClassSkillAssessments_Users_AssessedBy",
                table: "BabyClassSkillAssessments");

            migrationBuilder.DropIndex(
                name: "IX_BabyClassSkillAssessments_AssessedBy",
                table: "BabyClassSkillAssessments");

            migrationBuilder.RenameColumn(
                name: "BabyClassSkillId1",
                table: "BabyClassSkillAssessments",
                newName: "BabyClassSkillId");

            migrationBuilder.RenameIndex(
                name: "IX_BabyClassSkillAssessments_BabyClassSkillId1",
                table: "BabyClassSkillAssessments",
                newName: "IX_BabyClassSkillAssessments_BabyClassSkillId");

            migrationBuilder.AddForeignKey(
                name: "FK_BabyClassSkillAssessments_BabyClassSkills_BabyClassSkillId",
                table: "BabyClassSkillAssessments",
                column: "BabyClassSkillId",
                principalTable: "BabyClassSkills",
                principalColumn: "Id");
        }
    }
}
