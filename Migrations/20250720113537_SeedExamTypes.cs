using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BluebirdCore.Migrations
{
    /// <inheritdoc />
    public partial class SeedExamTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcademicYears",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExamTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Grades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Stream = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Section = table.Column<int>(type: "int", nullable: false),
                    HomeroomTeacherId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grades_Users_HomeroomTeacherId",
                        column: x => x.HomeroomTeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "GradeSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GradeId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradeSubjects_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GradeSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StudentNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    GuardianName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GuardianPhone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    GradeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    EnrollmentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArchiveDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Students_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TeacherSubjectAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherSubjectAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeacherSubjectAssignments_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherSubjectAssignments_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherSubjectAssignments_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    ExamTypeId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AcademicYear = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RecordedBy = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CommentsUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CommentsUpdatedBy = table.Column<int>(type: "int", nullable: true),
                    CommentsUpdatedByTeacherId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExamScores_ExamTypes_ExamTypeId",
                        column: x => x.ExamTypeId,
                        principalTable: "ExamTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamScores_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamScores_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamScores_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ExamScores_Users_CommentsUpdatedByTeacherId",
                        column: x => x.CommentsUpdatedByTeacherId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamScores_Users_RecordedBy",
                        column: x => x.RecordedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReportCards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    GradeId = table.Column<int>(type: "int", nullable: false),
                    AcademicYear = table.Column<int>(type: "int", nullable: false),
                    Term = table.Column<int>(type: "int", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportCards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportCards_Grades_GradeId",
                        column: x => x.GradeId,
                        principalTable: "Grades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportCards_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReportCards_Users_GeneratedBy",
                        column: x => x.GeneratedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StudentOptionalSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    EnrolledAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentOptionalSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentOptionalSubjects_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentOptionalSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AcademicYears",
                columns: new[] { "Id", "EndDate", "IsActive", "IsClosed", "Name", "StartDate" },
                values: new object[] { 1, new DateTime(2025, 12, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, false, "2025", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "ExamTypes",
                columns: new[] { "Id", "Description", "IsActive", "Name", "Order" },
                values: new object[,]
                {
                    { 1, "First test of the term", true, "Test-One", 1 },
                    { 2, "Second-test examination", true, "Test-Two", 2 },
                    { 3, "Third-test examination ( Usually taken by Grade 7)", true, "Test-Three", 3 },
                    { 4, "End of term examination", true, "End-of-Term", 4 }
                });

            migrationBuilder.InsertData(
                table: "Grades",
                columns: new[] { "Id", "HomeroomTeacherId", "IsActive", "Level", "Name", "Section", "Stream" },
                values: new object[,]
                {
                    { 1, null, true, 1, "Baby-Class", 0, "Purple" },
                    { 2, null, true, 1, "Baby-Class", 0, "Green" },
                    { 3, null, true, 1, "Baby-Class", 0, "Orange" },
                    { 4, null, true, 2, "Middle-Class", 1, "Purple" },
                    { 5, null, true, 2, "Middle-Class", 1, "Green" },
                    { 6, null, true, 2, "Middle-Class", 1, "Orange" },
                    { 7, null, true, 3, "Reception-Class", 1, "Purple" },
                    { 8, null, true, 3, "Reception-Class", 1, "Green" },
                    { 9, null, true, 3, "Reception-Class", 1, "Orange" },
                    { 10, null, true, 4, "Grade 1", 2, "Purple" },
                    { 11, null, true, 4, "Grade 1", 2, "Green" },
                    { 12, null, true, 4, "Grade 1", 2, "Orange" },
                    { 13, null, true, 5, "Grade 2", 2, "Purple" },
                    { 14, null, true, 5, "Grade 2", 2, "Green" },
                    { 15, null, true, 5, "Grade 2", 2, "Orange" },
                    { 16, null, true, 6, "Grade 3", 2, "Purple" },
                    { 17, null, true, 6, "Grade 3", 2, "Green" },
                    { 18, null, true, 6, "Grade 3", 2, "Orange" },
                    { 19, null, true, 7, "Grade 4", 3, "Purple" },
                    { 20, null, true, 7, "Grade 4", 3, "Green" },
                    { 21, null, true, 7, "Grade 4", 3, "Orange" },
                    { 22, null, true, 8, "Grade 5", 3, "Purple" },
                    { 23, null, true, 8, "Grade 5", 3, "Green" },
                    { 24, null, true, 8, "Grade 5", 3, "Orange" },
                    { 25, null, true, 9, "Grade 6", 3, "Purple" },
                    { 26, null, true, 9, "Grade 6", 3, "Green" },
                    { 27, null, true, 9, "Grade 6", 3, "Orange" },
                    { 28, null, true, 10, "Grade 7", 3, "Purple" },
                    { 29, null, true, 10, "Grade 7", 3, "Green" },
                    { 30, null, true, 10, "Grade 7", 3, "Orange" },
                    { 31, null, true, 11, "Form 1", 4, "Grey" },
                    { 32, null, true, 11, "Form 1", 4, "Blue" },
                    { 33, null, true, 11, "Grade 9", 4, "Grey" },
                    { 34, null, true, 11, "Grade 9", 4, "Blue" },
                    { 35, null, true, 12, "Grade 10", 5, "Grey" },
                    { 36, null, true, 12, "Grade 10", 5, "Blue" },
                    { 37, null, true, 13, "Grade 11", 5, "Grey" },
                    { 38, null, true, 13, "Grade 11", 5, "Blue" },
                    { 39, null, true, 14, "Grade 12", 5, "Grey" },
                    { 40, null, true, 14, "Grade 12", 5, "Blue" }
                });

            migrationBuilder.InsertData(
                table: "Subjects",
                columns: new[] { "Id", "Code", "Description", "IsActive", "Name" },
                values: new object[,]
                {
                    { 1, "MATH", null, true, "Mathematics" },
                    { 2, "ENG", null, true, "English" },
                    { 3, "SCI", null, true, "Integrated Science" },
                    { 4, "SS", null, true, "Social Studies" },
                    { 5, "FR", null, true, "French" },
                    { 6, "ICT", null, true, "ICT" },
                    { 7, "PE", null, true, "Physical Education" },
                    { 8, "REA", null, true, "Reading" },
                    { 9, "SP1", null, true, "SP1" },
                    { 10, "SP2", null, true, "SP2" },
                    { 11, "CTS", null, true, "CTS" },
                    { 12, "AGRI", null, true, "Agriculture Science" },
                    { 13, "HIST", null, true, "History" },
                    { 14, "GEO", null, true, "Geography" },
                    { 15, "REL", null, true, "Religious Studies" },
                    { 16, "BUS", null, true, "Business Studies" },
                    { 17, "ACC", null, true, "Accounts" },
                    { 18, "ECO", null, true, "Economics" },
                    { 19, "PHY", null, true, "Physics" },
                    { 20, "CHEM", null, true, "Chemistry" },
                    { 21, "BIO", null, true, "Biology" },
                    { 22, "LIT", null, true, "Literature in English" },
                    { 23, "CIV", null, true, "Civic Education" },
                    { 24, "MUS", null, true, "Music" },
                    { 25, "MDE", null, true, "MDE" },
                    { 26, "HE", null, true, "Home Economics" },
                    { 27, "COMMSK", null, true, "Communication Skills" },
                    { 28, "FM", null, true, "Fine-motor Skills" },
                    { 29, "GMS", null, true, "Gross-motor Skills" },
                    { 30, "SESK", null, true, "Social-emotional Skills" },
                    { 31, "COLSS", null, true, "Colors & Shapes" },
                    { 32, "NUMB", null, true, "Numbers" },
                    { 33, "CS", null, true, "Computer Studies" },
                    { 34, "COMSCI", null, true, "Computer Science" },
                    { 35, "CINY", null, true, "Cinyanja" },
                    { 36, "SCIE", null, true, "Science" },
                    { 37, "COMMER", null, true, "Commerce" },
                    { 38, "FRENCH", null, true, "French" },
                    { 39, "ART", null, true, "Art & Design" },
                    { 40, "F&N", null, true, "Food and Nutrition" },
                    { 41, "LANG", null, true, "Language" },
                    { 42, "PREMATH", null, true, "Pre-Math" },
                    { 43, "TOPIC", null, true, "Topic" },
                    { 44, "ELCCTS", null, true, "Creative And Technology Studies (CTS)" },
                    { 45, "CREAT", null, true, "Creative Activities" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2025, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@chsschool.com", "System Administrator", true, null, "$2a$12$Y5Cr10SW4OuJq6qxj7PXtOhZvb7loVQqIRRwcrH8hsdsoeRCririq", 1, "admin" });

            migrationBuilder.InsertData(
                table: "GradeSubjects",
                columns: new[] { "Id", "GradeId", "IsActive", "IsOptional", "SubjectId" },
                values: new object[,]
                {
                    { 1, 10, true, false, 1 },
                    { 2, 10, true, false, 2 },
                    { 3, 10, true, false, 11 },
                    { 4, 10, true, false, 3 },
                    { 5, 10, true, false, 4 },
                    { 6, 10, true, false, 25 },
                    { 7, 10, true, false, 5 },
                    { 8, 10, true, false, 35 },
                    { 9, 11, true, false, 1 },
                    { 10, 11, true, false, 2 },
                    { 11, 11, true, false, 11 },
                    { 12, 11, true, false, 3 },
                    { 13, 11, true, false, 4 },
                    { 14, 11, true, false, 25 },
                    { 15, 11, true, false, 5 },
                    { 16, 11, true, false, 35 },
                    { 17, 12, true, false, 1 },
                    { 18, 12, true, false, 2 },
                    { 19, 12, true, false, 11 },
                    { 20, 12, true, false, 3 },
                    { 21, 12, true, false, 4 },
                    { 22, 12, true, false, 25 },
                    { 23, 12, true, false, 5 },
                    { 24, 12, true, false, 35 },
                    { 25, 13, true, false, 1 },
                    { 26, 13, true, false, 2 },
                    { 27, 13, true, false, 11 },
                    { 28, 13, true, false, 3 },
                    { 29, 13, true, false, 4 },
                    { 30, 13, true, false, 25 },
                    { 31, 13, true, false, 5 },
                    { 32, 13, true, false, 35 },
                    { 33, 14, true, false, 1 },
                    { 34, 14, true, false, 2 },
                    { 35, 14, true, false, 11 },
                    { 36, 14, true, false, 3 },
                    { 37, 14, true, false, 4 },
                    { 38, 14, true, false, 25 },
                    { 39, 14, true, false, 5 },
                    { 40, 14, true, false, 35 },
                    { 41, 15, true, false, 1 },
                    { 42, 15, true, false, 2 },
                    { 43, 15, true, false, 11 },
                    { 44, 15, true, false, 3 },
                    { 45, 15, true, false, 4 },
                    { 46, 15, true, false, 25 },
                    { 47, 15, true, false, 5 },
                    { 48, 15, true, false, 35 },
                    { 49, 16, true, false, 1 },
                    { 50, 16, true, false, 2 },
                    { 51, 16, true, false, 11 },
                    { 52, 16, true, false, 3 },
                    { 53, 16, true, false, 4 },
                    { 54, 16, true, false, 25 },
                    { 55, 16, true, false, 5 },
                    { 56, 16, true, false, 35 },
                    { 57, 17, true, false, 1 },
                    { 58, 17, true, false, 2 },
                    { 59, 17, true, false, 11 },
                    { 60, 17, true, false, 3 },
                    { 61, 17, true, false, 4 },
                    { 62, 17, true, false, 25 },
                    { 63, 17, true, false, 5 },
                    { 64, 17, true, false, 35 },
                    { 65, 18, true, false, 1 },
                    { 66, 18, true, false, 2 },
                    { 67, 18, true, false, 11 },
                    { 68, 18, true, false, 3 },
                    { 69, 18, true, false, 4 },
                    { 70, 18, true, false, 25 },
                    { 71, 18, true, false, 5 },
                    { 72, 18, true, false, 35 },
                    { 73, 19, true, false, 1 },
                    { 74, 19, true, false, 2 },
                    { 75, 19, true, false, 11 },
                    { 76, 19, true, false, 3 },
                    { 77, 19, true, false, 4 },
                    { 78, 19, true, false, 25 },
                    { 79, 19, true, false, 5 },
                    { 80, 19, true, false, 35 },
                    { 81, 19, true, false, 9 },
                    { 82, 19, true, false, 10 },
                    { 83, 20, true, false, 1 },
                    { 84, 20, true, false, 2 },
                    { 85, 20, true, false, 11 },
                    { 86, 20, true, false, 3 },
                    { 87, 20, true, false, 4 },
                    { 88, 20, true, false, 25 },
                    { 89, 20, true, false, 5 },
                    { 90, 20, true, false, 35 },
                    { 91, 20, true, false, 9 },
                    { 92, 20, true, false, 10 },
                    { 93, 21, true, false, 1 },
                    { 94, 21, true, false, 2 },
                    { 95, 21, true, false, 11 },
                    { 96, 21, true, false, 3 },
                    { 97, 21, true, false, 4 },
                    { 98, 21, true, false, 25 },
                    { 99, 21, true, false, 5 },
                    { 100, 21, true, false, 35 },
                    { 101, 21, true, false, 9 },
                    { 102, 21, true, false, 10 },
                    { 103, 22, true, false, 1 },
                    { 104, 22, true, false, 2 },
                    { 105, 22, true, false, 11 },
                    { 106, 22, true, false, 3 },
                    { 107, 22, true, false, 4 },
                    { 108, 22, true, false, 25 },
                    { 109, 22, true, false, 5 },
                    { 110, 22, true, false, 35 },
                    { 111, 22, true, false, 9 },
                    { 112, 22, true, false, 10 },
                    { 113, 23, true, false, 1 },
                    { 114, 23, true, false, 2 },
                    { 115, 23, true, false, 11 },
                    { 116, 23, true, false, 3 },
                    { 117, 23, true, false, 4 },
                    { 118, 23, true, false, 25 },
                    { 119, 23, true, false, 5 },
                    { 120, 23, true, false, 35 },
                    { 121, 23, true, false, 9 },
                    { 122, 23, true, false, 10 },
                    { 123, 24, true, false, 1 },
                    { 124, 24, true, false, 2 },
                    { 125, 24, true, false, 11 },
                    { 126, 24, true, false, 3 },
                    { 127, 24, true, false, 4 },
                    { 128, 24, true, false, 25 },
                    { 129, 24, true, false, 5 },
                    { 130, 24, true, false, 35 },
                    { 131, 24, true, false, 9 },
                    { 132, 24, true, false, 10 },
                    { 133, 25, true, false, 1 },
                    { 134, 25, true, false, 2 },
                    { 135, 25, true, false, 11 },
                    { 136, 25, true, false, 3 },
                    { 137, 25, true, false, 4 },
                    { 138, 25, true, false, 25 },
                    { 139, 25, true, false, 5 },
                    { 140, 25, true, false, 35 },
                    { 141, 25, true, false, 9 },
                    { 142, 25, true, false, 10 },
                    { 143, 26, true, false, 1 },
                    { 144, 26, true, false, 2 },
                    { 145, 26, true, false, 11 },
                    { 146, 26, true, false, 3 },
                    { 147, 26, true, false, 4 },
                    { 148, 26, true, false, 25 },
                    { 149, 26, true, false, 5 },
                    { 150, 26, true, false, 35 },
                    { 151, 26, true, false, 9 },
                    { 152, 26, true, false, 10 },
                    { 153, 27, true, false, 1 },
                    { 154, 27, true, false, 2 },
                    { 155, 27, true, false, 11 },
                    { 156, 27, true, false, 3 },
                    { 157, 27, true, false, 4 },
                    { 158, 27, true, false, 25 },
                    { 159, 27, true, false, 5 },
                    { 160, 27, true, false, 35 },
                    { 161, 27, true, false, 9 },
                    { 162, 27, true, false, 10 },
                    { 163, 28, true, false, 1 },
                    { 164, 28, true, false, 2 },
                    { 165, 28, true, false, 11 },
                    { 166, 28, true, false, 3 },
                    { 167, 28, true, false, 4 },
                    { 168, 28, true, false, 25 },
                    { 169, 28, true, false, 5 },
                    { 170, 28, true, false, 35 },
                    { 171, 28, true, false, 9 },
                    { 172, 28, true, false, 10 },
                    { 173, 29, true, false, 1 },
                    { 174, 29, true, false, 2 },
                    { 175, 29, true, false, 11 },
                    { 176, 29, true, false, 3 },
                    { 177, 29, true, false, 4 },
                    { 178, 29, true, false, 25 },
                    { 179, 29, true, false, 5 },
                    { 180, 29, true, false, 35 },
                    { 181, 29, true, false, 9 },
                    { 182, 29, true, false, 10 },
                    { 183, 30, true, false, 1 },
                    { 184, 30, true, false, 2 },
                    { 185, 30, true, false, 11 },
                    { 186, 30, true, false, 3 },
                    { 187, 30, true, false, 4 },
                    { 188, 30, true, false, 25 },
                    { 189, 30, true, false, 5 },
                    { 190, 30, true, false, 35 },
                    { 191, 30, true, false, 9 },
                    { 192, 30, true, false, 10 },
                    { 193, 31, true, false, 2 },
                    { 194, 31, true, false, 1 },
                    { 195, 31, true, false, 4 },
                    { 198, 31, true, false, 25 },
                    { 199, 31, true, false, 6 },
                    { 200, 31, true, true, 17 },
                    { 201, 31, true, true, 22 },
                    { 202, 31, true, true, 12 },
                    { 203, 31, true, true, 15 },
                    { 204, 31, true, true, 37 },
                    { 205, 31, true, true, 26 },
                    { 206, 31, true, true, 24 },
                    { 207, 31, true, true, 16 },
                    { 208, 31, true, true, 34 },
                    { 209, 31, true, true, 13 },
                    { 210, 31, true, true, 14 },
                    { 211, 31, true, true, 5 },
                    { 212, 31, true, true, 39 },
                    { 213, 32, true, false, 2 },
                    { 214, 32, true, false, 1 },
                    { 215, 32, true, false, 4 },
                    { 217, 32, true, false, 25 },
                    { 218, 32, true, false, 6 },
                    { 219, 32, true, true, 17 },
                    { 220, 32, true, true, 22 },
                    { 221, 32, true, true, 12 },
                    { 222, 32, true, true, 15 },
                    { 223, 32, true, true, 37 },
                    { 224, 32, true, true, 26 },
                    { 225, 32, true, true, 24 },
                    { 226, 32, true, true, 16 },
                    { 227, 32, true, true, 13 },
                    { 228, 32, true, true, 14 },
                    { 229, 32, true, true, 5 },
                    { 230, 32, true, true, 39 },
                    { 231, 33, true, false, 2 },
                    { 232, 33, true, false, 1 },
                    { 233, 33, true, false, 4 },
                    { 234, 33, true, false, 36 },
                    { 235, 33, true, false, 33 },
                    { 236, 33, true, false, 25 },
                    { 237, 33, true, true, 17 },
                    { 238, 33, true, true, 22 },
                    { 239, 33, true, true, 12 },
                    { 240, 33, true, true, 15 },
                    { 241, 33, true, true, 37 },
                    { 242, 33, true, true, 26 },
                    { 243, 33, true, true, 24 },
                    { 244, 33, true, true, 16 },
                    { 245, 33, true, true, 13 },
                    { 246, 33, true, true, 14 },
                    { 247, 33, true, true, 5 },
                    { 248, 33, true, true, 39 },
                    { 249, 34, true, false, 2 },
                    { 250, 34, true, false, 1 },
                    { 251, 34, true, false, 4 },
                    { 252, 34, true, false, 36 },
                    { 253, 34, true, false, 33 },
                    { 254, 34, true, false, 25 },
                    { 255, 34, true, true, 17 },
                    { 256, 34, true, true, 22 },
                    { 257, 34, true, true, 12 },
                    { 258, 34, true, true, 15 },
                    { 259, 34, true, true, 37 },
                    { 260, 34, true, true, 26 },
                    { 261, 34, true, true, 24 },
                    { 262, 34, true, true, 16 },
                    { 263, 34, true, true, 13 },
                    { 264, 34, true, true, 14 },
                    { 265, 34, true, true, 5 },
                    { 266, 34, true, true, 39 },
                    { 267, 35, true, false, 2 },
                    { 268, 35, true, false, 1 },
                    { 269, 35, true, false, 4 },
                    { 270, 35, true, false, 36 },
                    { 271, 35, true, false, 33 },
                    { 272, 35, true, false, 25 },
                    { 273, 35, true, true, 17 },
                    { 274, 35, true, true, 22 },
                    { 275, 35, true, true, 12 },
                    { 276, 35, true, true, 15 },
                    { 277, 35, true, true, 37 },
                    { 278, 35, true, true, 26 },
                    { 279, 35, true, true, 24 },
                    { 280, 35, true, true, 16 },
                    { 281, 35, true, true, 13 },
                    { 282, 35, true, true, 14 },
                    { 283, 35, true, true, 5 },
                    { 284, 35, true, true, 39 },
                    { 285, 36, true, false, 2 },
                    { 286, 36, true, false, 1 },
                    { 287, 36, true, false, 4 },
                    { 288, 36, true, false, 36 },
                    { 289, 36, true, false, 33 },
                    { 290, 36, true, false, 25 },
                    { 291, 36, true, true, 17 },
                    { 292, 36, true, true, 22 },
                    { 293, 36, true, true, 12 },
                    { 294, 36, true, true, 15 },
                    { 295, 36, true, true, 37 },
                    { 296, 36, true, true, 26 },
                    { 297, 36, true, true, 24 },
                    { 298, 36, true, true, 16 },
                    { 299, 36, true, true, 13 },
                    { 300, 36, true, true, 14 },
                    { 301, 36, true, true, 5 },
                    { 302, 36, true, true, 39 },
                    { 303, 37, true, false, 2 },
                    { 304, 37, true, false, 1 },
                    { 305, 37, true, false, 4 },
                    { 306, 37, true, false, 36 },
                    { 307, 37, true, false, 33 },
                    { 308, 37, true, false, 25 },
                    { 309, 37, true, true, 17 },
                    { 310, 37, true, true, 22 },
                    { 311, 37, true, true, 12 },
                    { 312, 37, true, true, 15 },
                    { 313, 37, true, true, 37 },
                    { 314, 37, true, true, 26 },
                    { 315, 37, true, true, 24 },
                    { 316, 37, true, true, 16 },
                    { 317, 37, true, true, 13 },
                    { 318, 37, true, true, 14 },
                    { 319, 37, true, true, 5 },
                    { 320, 37, true, true, 39 },
                    { 321, 38, true, false, 2 },
                    { 322, 38, true, false, 1 },
                    { 323, 38, true, false, 4 },
                    { 324, 38, true, false, 36 },
                    { 325, 38, true, false, 33 },
                    { 326, 38, true, false, 25 },
                    { 327, 38, true, true, 17 },
                    { 328, 38, true, true, 22 },
                    { 329, 38, true, true, 12 },
                    { 330, 38, true, true, 15 },
                    { 331, 38, true, true, 37 },
                    { 332, 38, true, true, 26 },
                    { 333, 38, true, true, 24 },
                    { 334, 38, true, true, 16 },
                    { 335, 38, true, true, 13 },
                    { 336, 38, true, true, 14 },
                    { 337, 38, true, true, 5 },
                    { 338, 38, true, true, 39 },
                    { 339, 39, true, false, 2 },
                    { 340, 39, true, false, 1 },
                    { 341, 39, true, false, 4 },
                    { 342, 39, true, false, 36 },
                    { 343, 39, true, false, 33 },
                    { 344, 39, true, false, 25 },
                    { 345, 39, true, true, 17 },
                    { 346, 39, true, true, 22 },
                    { 347, 39, true, true, 12 },
                    { 348, 39, true, true, 15 },
                    { 349, 39, true, true, 37 },
                    { 350, 39, true, true, 26 },
                    { 351, 39, true, true, 24 },
                    { 352, 39, true, true, 16 },
                    { 353, 39, true, true, 13 },
                    { 354, 39, true, true, 14 },
                    { 355, 39, true, true, 5 },
                    { 356, 39, true, true, 39 },
                    { 357, 40, true, false, 2 },
                    { 358, 40, true, false, 1 },
                    { 359, 40, true, false, 4 },
                    { 360, 40, true, false, 36 },
                    { 361, 40, true, false, 33 },
                    { 362, 40, true, false, 25 },
                    { 363, 40, true, true, 17 },
                    { 364, 40, true, true, 22 },
                    { 365, 40, true, true, 12 },
                    { 366, 40, true, true, 15 },
                    { 367, 40, true, true, 37 },
                    { 368, 40, true, true, 26 },
                    { 369, 40, true, true, 24 },
                    { 370, 40, true, true, 16 },
                    { 371, 40, true, true, 13 },
                    { 372, 40, true, true, 14 },
                    { 373, 40, true, true, 5 },
                    { 374, 40, true, true, 39 },
                    { 375, 1, true, false, 40 },
                    { 376, 1, true, false, 41 },
                    { 377, 1, true, false, 42 },
                    { 378, 1, true, false, 44 },
                    { 379, 2, true, false, 40 },
                    { 380, 2, true, false, 41 },
                    { 381, 2, true, false, 42 },
                    { 382, 2, true, false, 44 },
                    { 383, 3, true, false, 40 },
                    { 384, 3, true, false, 41 },
                    { 385, 3, true, false, 42 },
                    { 386, 3, true, false, 44 },
                    { 387, 4, true, false, 40 },
                    { 388, 4, true, false, 41 },
                    { 389, 4, true, false, 42 },
                    { 390, 4, true, false, 43 },
                    { 391, 5, true, false, 40 },
                    { 392, 5, true, false, 41 },
                    { 393, 5, true, false, 42 },
                    { 394, 5, true, false, 43 },
                    { 395, 6, true, false, 40 },
                    { 396, 6, true, false, 41 },
                    { 397, 6, true, false, 42 },
                    { 398, 6, true, false, 43 },
                    { 399, 7, true, false, 40 },
                    { 400, 7, true, false, 41 },
                    { 401, 7, true, false, 42 },
                    { 402, 7, true, false, 43 },
                    { 403, 7, true, false, 5 },
                    { 404, 8, true, false, 40 },
                    { 405, 8, true, false, 41 },
                    { 406, 8, true, false, 42 },
                    { 407, 8, true, false, 43 },
                    { 408, 8, true, false, 5 },
                    { 409, 9, true, false, 40 },
                    { 410, 9, true, false, 41 },
                    { 411, 9, true, false, 42 },
                    { 412, 9, true, false, 43 },
                    { 413, 9, true, false, 5 },
                    { 3779, 31, true, false, 21 },
                    { 3780, 32, true, false, 21 },
                    { 4000, 10, true, false, 8 },
                    { 4001, 11, true, false, 8 },
                    { 4002, 12, true, false, 8 },
                    { 4003, 13, true, false, 8 },
                    { 4004, 14, true, false, 8 },
                    { 4005, 15, true, false, 8 },
                    { 4006, 16, true, false, 8 },
                    { 4007, 17, true, false, 8 },
                    { 4008, 18, true, false, 8 },
                    { 4009, 19, true, false, 8 },
                    { 4010, 20, true, false, 8 },
                    { 4011, 21, true, false, 8 },
                    { 4012, 22, true, false, 8 },
                    { 4013, 23, true, false, 8 },
                    { 4014, 24, true, false, 8 },
                    { 4015, 25, true, false, 8 },
                    { 4016, 26, true, false, 8 },
                    { 4017, 27, true, false, 8 },
                    { 4018, 28, true, false, 8 },
                    { 4019, 29, true, false, 8 },
                    { 4020, 30, true, false, 8 },
                    { 4021, 31, true, false, 8 },
                    { 4022, 32, true, false, 8 },
                    { 4023, 33, true, false, 8 },
                    { 4024, 34, true, false, 8 },
                    { 4025, 35, true, false, 8 },
                    { 4026, 36, true, false, 8 },
                    { 4027, 37, true, false, 8 },
                    { 4028, 38, true, false, 8 },
                    { 4029, 39, true, false, 8 },
                    { 4030, 40, true, false, 8 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExamScores_CommentsUpdatedByTeacherId",
                table: "ExamScores",
                column: "CommentsUpdatedByTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScores_ExamTypeId",
                table: "ExamScores",
                column: "ExamTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScores_GradeId",
                table: "ExamScores",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScores_RecordedBy",
                table: "ExamScores",
                column: "RecordedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ExamScores_StudentId_SubjectId_ExamTypeId_AcademicYear_Term",
                table: "ExamScores",
                columns: new[] { "StudentId", "SubjectId", "ExamTypeId", "AcademicYear", "Term" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExamScores_SubjectId",
                table: "ExamScores",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Grades_HomeroomTeacherId",
                table: "Grades",
                column: "HomeroomTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_GradeSubjects_GradeId_SubjectId",
                table: "GradeSubjects",
                columns: new[] { "GradeId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradeSubjects_SubjectId",
                table: "GradeSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportCards_GeneratedBy",
                table: "ReportCards",
                column: "GeneratedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReportCards_GradeId",
                table: "ReportCards",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportCards_StudentId",
                table: "ReportCards",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentOptionalSubjects_StudentId_SubjectId",
                table: "StudentOptionalSubjects",
                columns: new[] { "StudentId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentOptionalSubjects_SubjectId",
                table: "StudentOptionalSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_GradeId",
                table: "Students",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_StudentNumber",
                table: "Students",
                column: "StudentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectAssignments_GradeId",
                table: "TeacherSubjectAssignments",
                column: "GradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectAssignments_SubjectId",
                table: "TeacherSubjectAssignments",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherSubjectAssignments_TeacherId",
                table: "TeacherSubjectAssignments",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademicYears");

            migrationBuilder.DropTable(
                name: "ExamScores");

            migrationBuilder.DropTable(
                name: "GradeSubjects");

            migrationBuilder.DropTable(
                name: "ReportCards");

            migrationBuilder.DropTable(
                name: "StudentOptionalSubjects");

            migrationBuilder.DropTable(
                name: "TeacherSubjectAssignments");

            migrationBuilder.DropTable(
                name: "ExamTypes");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
