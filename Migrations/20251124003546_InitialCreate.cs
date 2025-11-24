using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SchoolErpSMS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
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
                    AssessedBy = table.Column<int>(type: "int", nullable: false)
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
                        name: "FK_BabyClassSkillAssessments_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BabyClassSkillAssessments_Users_AssessedBy",
                        column: x => x.AssessedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    IsAbsent = table.Column<bool>(type: "bit", nullable: false),
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
                    GeneratedBy = table.Column<int>(type: "int", nullable: false),
                    GeneralComment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    GeneralCommentUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneralCommentUpdatedBy = table.Column<int>(type: "int", nullable: true),
                    GeneralCommentUpdatedByUserId = table.Column<int>(type: "int", nullable: true)
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
                        name: "FK_ReportCards_Users_GeneralCommentUpdatedByUserId",
                        column: x => x.GeneralCommentUpdatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportCards_Users_GeneratedBy",
                        column: x => x.GeneratedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateTable(
                name: "StudentSubjects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    EnrolledDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DroppedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentSubjects_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentTechnologyTracks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    TrackName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SelectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTechnologyTracks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTechnologyTracks_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
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
                    { 3, "End of term examination", true, "End-of-Term", 3 }
                });

            migrationBuilder.InsertData(
                table: "Grades",
                columns: new[] { "Id", "HomeroomTeacherId", "IsActive", "Level", "Name", "Section", "Stream" },
                values: new object[,]
                {
                    { 1, null, true, 11, "Form 1", 4, "W" },
                    { 2, null, true, 11, "Form 1", 4, "X" },
                    { 3, null, true, 11, "Form 1", 4, "Y" },
                    { 4, null, true, 12, "Grade 10", 5, "V" },
                    { 5, null, true, 12, "Grade 10", 5, "W" },
                    { 6, null, true, 12, "Grade 10", 5, "X" },
                    { 7, null, true, 12, "Grade 10", 5, "Y" },
                    { 8, null, true, 13, "Grade 11", 5, "V" },
                    { 9, null, true, 13, "Grade 11", 5, "W" },
                    { 10, null, true, 13, "Grade 11", 5, "X" },
                    { 11, null, true, 13, "Grade 11", 5, "Y" },
                    { 12, null, true, 14, "Grade 12", 5, "V" },
                    { 13, null, true, 14, "Grade 12", 5, "W" },
                    { 14, null, true, 14, "Grade 12", 5, "X" },
                    { 15, null, true, 14, "Grade 12", 5, "Y" }
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
                    { 39, "ART", null, true, "Art & Design" },
                    { 40, "F&N", null, true, "Food and Nutrition" },
                    { 41, "LANG", null, true, "Language" },
                    { 42, "PREMATH", null, true, "Pre-Math" },
                    { 43, "TOPIC", null, true, "Topic" },
                    { 44, "ELCCTS", null, true, "Creative And Technology Studies (CTS)" },
                    { 45, "CREAT", null, true, "Creative Activities" },
                    { 46, "D&T", null, true, "Design and Technology" },
                    { 47, "F&F", null, true, "Fashion and Fabrics" },
                    { 48, "HOSP", null, true, "Hospitality Management" },
                    { 49, "T&T", null, true, "Travel and Tourism" },
                    { 50, "FL", null, true, "Foreign Language" },
                    { 51, "ZL", null, true, "Zambian Language" },
                    { 52, "POA", null, true, "Principles of Accounts" },
                    { 53, "LITZL", null, true, "Literature in Zambian Language" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "FullName", "IsActive", "LastLoginAt", "PasswordHash", "Role", "Username" },
                values: new object[] { 1, new DateTime(2025, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@scherp.sch.edu", "System Admin", true, null, "$2a$11$I0Q7cC9y7.NTwp3hWV3QnOfYsVkRZi1ZsRa1IVGck5VIriBdoip.O", 1, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_AssessedBy",
                table: "BabyClassSkillAssessments",
                column: "AssessedBy");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_SkillItemId",
                table: "BabyClassSkillAssessments",
                column: "SkillItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillAssessments_StudentId",
                table: "BabyClassSkillAssessments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkillItems_SkillId",
                table: "BabyClassSkillItems",
                column: "SkillId");

            migrationBuilder.CreateIndex(
                name: "IX_BabyClassSkills_Name",
                table: "BabyClassSkills",
                column: "Name",
                unique: true);

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
                name: "IX_ReportCards_GeneralCommentUpdatedByUserId",
                table: "ReportCards",
                column: "GeneralCommentUpdatedByUserId");

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
                name: "IX_StudentSubjects_StudentId_SubjectId",
                table: "StudentSubjects",
                columns: new[] { "StudentId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentSubjects_SubjectId",
                table: "StudentSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTechnologyTracks_StudentId",
                table: "StudentTechnologyTracks",
                column: "StudentId");

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
                name: "BabyClassSkillAssessments");

            migrationBuilder.DropTable(
                name: "ExamScores");

            migrationBuilder.DropTable(
                name: "GradeSubjects");

            migrationBuilder.DropTable(
                name: "ReportCards");

            migrationBuilder.DropTable(
                name: "SmsLogs");

            migrationBuilder.DropTable(
                name: "StudentOptionalSubjects");

            migrationBuilder.DropTable(
                name: "StudentSubjects");

            migrationBuilder.DropTable(
                name: "StudentTechnologyTracks");

            migrationBuilder.DropTable(
                name: "TeacherSubjectAssignments");

            migrationBuilder.DropTable(
                name: "BabyClassSkillItems");

            migrationBuilder.DropTable(
                name: "ExamTypes");

            migrationBuilder.DropTable(
                name: "AcademicYears");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "BabyClassSkills");

            migrationBuilder.DropTable(
                name: "Grades");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
