using System.ComponentModel.DataAnnotations;

namespace BluebirdCore.DTOs
{
    // ===== HOMEROOM TEACHER DTOs =====
    
    public class HomeroomStudentDto
    {
        public int Id { get; set; }
        public required string StudentNumber { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string FullName { get; set; }
        public required string GradeName { get; set; }
        public int GradeId { get; set; }
        public List<StudentSubjectDto> Subjects { get; set; } = new List<StudentSubjectDto>();
    }


    public class AssignSubjectDto
    {
        [Required]
        public int SubjectId { get; set; }
        public string? Notes { get; set; }
    }

    public class BulkAssignSubjectsDto
    {
        [Required]
        public List<int> StudentIds { get; set; } = new List<int>();
        [Required]
        public int SubjectId { get; set; }
        public string? Notes { get; set; }
    }

    public class HomeroomGradeInfoDto
    {
        public int GradeId { get; set; }
        public required string GradeName { get; set; }
        public required string Section { get; set; }
        public int Level { get; set; }
        public int StudentCount { get; set; }
        public List<AvailableSubjectDto> AvailableSubjects { get; set; } = new List<AvailableSubjectDto>();
    }

    public class AvailableSubjectDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Code { get; set; }
        public string? Description { get; set; }
        public bool IsOptional { get; set; }
        public bool IsAssigned { get; set; }
    }

    public class RemoveSubjectDto
    {
        public string? Reason { get; set; }
    }

    public class UpdateStudentNameDto
    {
        [Required]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [StringLength(50)]
        public string? MiddleName { get; set; }

        [Required]
        [StringLength(50)]
        public required string LastName { get; set; }
    }
}
