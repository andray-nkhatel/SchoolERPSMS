using System.ComponentModel.DataAnnotations;
using SchoolErpSMS.Entities;

namespace SchoolErpSMS.DTOs
{
    public class LoginDto
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }

    public class LoginResponseDto
    {
        public string? Token { get; set; }
        public UserDto? User { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        public UserRole Role { get; set; }

        [Required]
        [MinLength(6)]
        public string? Password { get; set; }
    }

    public class StudentDto
    {
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? StudentNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? GuardianName { get; set; }
        public string? GuardianPhone { get; set; }
        public int GradeId { get; set; }
        public string? GradeName { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string? FullName { get; set; }
        public List<SubjectDto>? OptionalSubjects { get; set; }
    }

    public class CreateStudentDto
    {
        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }

        [StringLength(50)]
        public string? MiddleName { get; set; }

        [StringLength(20)]
        public string StudentNumber { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [StringLength(10)]
        public required string Gender { get; set; }

        [StringLength(200)]
        public string Address { get; set; }

        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [StringLength(100)]
        public string GuardianName { get; set; }

        [StringLength(15)]
        public string GuardianPhone { get; set; }

        [Required]
        public int GradeId { get; set; }
    }

    public class SubjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class GradeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Stream { get; set; }
        public string FullName { get; set; }
        public int Level { get; set; }
        public string Section { get; set; }
        public int? HomeroomTeacherId { get; set; }
        public string HomeroomTeacherName { get; set; }
        public bool IsActive { get; set; }
        public int StudentCount { get; set; }
    }

    public class ExamScoreDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int ExamTypeId { get; set; }
        public string ExamTypeName { get; set; }
        public decimal Score { get; set; }
        public bool IsAbsent { get; set; }
        public int AcademicYear { get; set; }
        public int Term { get; set; }
        public DateTime RecordedAt { get; set; }
        public string RecordedByName { get; set; }
        // Comment fields
        public string? Comments { get; set; }
        public DateTime? CommentsUpdatedAt { get; set; }
        public string? CommentsUpdatedByName { get; set; }
    }

    public class CreateExamScoreDto
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [Required]
        public int ExamTypeId { get; set; }

        [Required]
        [Range(0, 150)]
        public decimal Score { get; set; }

        public bool IsAbsent { get; set; } = false;

        [Required]
        public int AcademicYear { get; set; }

        [Required]
        [Range(1, 3)]
        public int Term { get; set; }

        // [Required]
        [StringLength(1000)]
        public string? Comments { get; set; }
    }

    // For updating existing scores
    public class UpdateExamScoreDto
    {
        [Required]
        [Range(0, 150, ErrorMessage = "Score must be between 0 and 100")]
        public decimal Score { get; set; }

        public bool IsAbsent { get; set; } = false;

        [StringLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters")]
        public string? Comments { get; set; }
    }

    // For updating exam types
    public class UpdateExamTypeDto
    {
        [Required]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public string Name { get; set; }

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; }

        [Range(1, 100, ErrorMessage = "Order must be between 1 and 100")]
        public int Order { get; set; }
    }


    public class ReportCardDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string GradeName { get; set; }
        public int AcademicYear { get; set; }
        public int Term { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string GeneratedByName { get; set; }
    }

    public class ResetPasswordDto
    {
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }

    public class PromoteStudentsDto
    {
        [Required]
        public int FromGradeId { get; set; }

        [Required]
        public int ToGradeId { get; set; }
    }

    public class CreateGradeDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(50)]
        public string Stream { get; set; }

        [Required]
        public int Level { get; set; }

        [Required]
        public SchoolSection Section { get; set; }

        public int? HomeroomTeacherId { get; set; }
    }

    public class AssignHomeroomTeacherDto
    {
        [Required]
        public int TeacherId { get; set; }
    }

    public class CreateSubjectDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(10)]
        public string Code { get; set; }

        [StringLength(500)]
        public string Description { get; set; }
    }

    public class AssignSubjectToGradeDto
    {
        public bool IsOptional { get; set; } = false;
        public bool AutoAssignToStudents { get; set; } = false;
        public bool AssignToExistingStudents { get; set; } = false;
        public int? AcademicYearId { get; set; }
    }

    public class BulkAssignSubjectsToGradeDto
    {
        [Required]
        public List<int> SubjectIds { get; set; } = new List<int>();
        public bool AutoAssignToStudents { get; set; } = false;
        public bool AssignToExistingStudents { get; set; } = false;
        public int? AcademicYearId { get; set; }
    }

    public class SyncGradeStudentSubjectsDto
    {
        public int? AcademicYearId { get; set; }
        public bool RemoveOrphaned { get; set; } = false;
    }

    public class AssignTeacherToSubjectDto
    {
        [Required]
        public int TeacherId { get; set; }

        [Required]
        public int GradeId { get; set; }
    }

    public class CreateExamTypeDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public int Order { get; set; }
    }

    public class TeacherAssignmentDto
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public SchoolSection? GradeSection { get; set; }
        public DateTime AssignedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateAcademicYearDto
    {
        [Required]
        [StringLength(20)]
        public string Name { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
    }

    public class UpdateStudentDto
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string StudentNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string GuardianName { get; set; }
        public string GuardianPhone { get; set; }
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public bool IsActive { get; set; }
        //public bool IsArchived { get; set; }
        //public List<SubjectDto> OptionalSubjects { get; set; }
        // Add other fields as needed
    }

    public class UpdateStudentCurriculumDto
    {
        public int? FormLevel { get; set; } // Form level (1-6) for new curriculum students, null for Legacy
        public int AcademicYear { get; set; } = 2025; // Academic year for curriculum determination
        public string CurriculumType { get; set; } = "Legacy"; // "Legacy" or "New"
    }

    public class UpdateGradeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Stream { get; set; } = string.Empty;
        public int Level { get; set; }
        public SchoolSection Section { get; set; }
        public int? HomeroomTeacherId { get; set; }
    }

    public class UpdateSubjectDto
    {
        [Required(ErrorMessage = "Subject name is required")]
        [StringLength(100, ErrorMessage = "Subject name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject code is required")]
        [StringLength(10, ErrorMessage = "Subject code cannot exceed 10 characters")]
        public string Code { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // UpdateAcademicYearDto.cs
    public class UpdateAcademicYearDto
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class BulkAssignmentDto
    {
        public List<TeacherSubjectGradeAssignment> Assignments { get; set; }
    }

    public class TeacherSubjectGradeAssignment
    {
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }
        public int GradeId { get; set; }
    }

    public class AssignTeacherToSubjectMultipleGradesDto
    {
        public int TeacherId { get; set; }
        public List<int> GradeIds { get; set; } = new List<int>();
    }

    public class AssignOptionalSubjectsDto
    {
        [Required]
        public List<int> SubjectIds { get; set; } = new List<int>();
    }

    public class BulkAssignOptionalSubjectsDto
    {
        [Required]
        public List<int> StudentIds { get; set; } = new List<int>();
        
        [Required]
        public List<int> SubjectIds { get; set; } = new List<int>();
    }

    public class BulkAssignmentResult
    {
        public int StudentId { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class OptionalSubjectEnrollmentStatus
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string? SubjectCode { get; set; }
        public List<StudentDto> EnrolledStudents { get; set; } = new List<StudentDto>();
        public List<StudentDto> UnenrolledStudents { get; set; } = new List<StudentDto>();
        public int EnrollmentCount { get; set; }
        public int TotalStudents { get; set; }
        public double EnrollmentPercentage { get; set; }
    }

    public class BulkRemoveOptionalSubjectsDto
    {
        [Required]
        public List<int> StudentIds { get; set; } = new List<int>();
    }

    public class BulkAbsentDto
    {
        [Required]
        public List<int> StudentIds { get; set; } = new List<int>();
        
        [Required]
        public int SubjectId { get; set; }
        
        [Required]
        public int ExamTypeId { get; set; }
        
        [Required]
        public int AcademicYear { get; set; }
        
        [Required]
        public int Term { get; set; }
        
        [Required]
        public bool IsAbsent { get; set; }
    }

    public class ImportResult<T>
    {
        public int Successful { get; set; }
        public int Failed { get; set; }
        public int Total { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<T> Imported { get; set; } = new();
    }

    public class CreateMinimalStudentDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required int GradeId { get; set; }
    }
    
    public class UpdateMinimalStudentDto
    {
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public int GradeId { get; set; }
        public bool IsActive { get; set; }
    }

    public class TeacherAssignmentDtoMinimal
    {
        public int AssignmentId { get; set; }
        public int SubjectId { get; set; }
        public int GradeId { get; set; }
        public int TeacherId { get; set; }
    }
    // Baby Class Skill Assessment DTOs
    public class BabyClassSkillDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public List<BabyClassSkillItemDto> SkillItems { get; set; } = new List<BabyClassSkillItemDto>();
    }

    public class BabyClassSkillItemDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int SkillId { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
    }

    public class BabyClassSkillAssessmentDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SkillItemId { get; set; }
        public int AcademicYear { get; set; }
        public int Term { get; set; }
        public string? TeacherComment { get; set; }
        public DateTime AssessedAt { get; set; }
        public int AssessedBy { get; set; }
        public string? StudentName { get; set; }
        public string? SkillItemName { get; set; }
        public string? SkillName { get; set; }
        public string? AssessedByTeacherName { get; set; }
    }

    public class CreateSkillAssessmentDto
    {
        [Required]
        public int StudentId { get; set; }
        
        [Required]
        public int SkillItemId { get; set; }
        
        [Required]
        public int AcademicYear { get; set; }
        
        [Required]
        public int Term { get; set; }
        
        public string? TeacherComment { get; set; }
    }

    public class UpdateSkillAssessmentDto
    {
        [Required]
        public int AssessmentId { get; set; }
        
        public string? TeacherComment { get; set; }
    }

    public class StudentSkillAssessmentSummaryDto
    {
        public int StudentId { get; set; }
        public required string StudentName { get; set; }
        public int GradeId { get; set; }
        public required string GradeName { get; set; }
        public List<BabyClassSkillAssessmentDto> Assessments { get; set; } = new List<BabyClassSkillAssessmentDto>();
    }





}