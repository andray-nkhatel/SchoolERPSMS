using System.ComponentModel.DataAnnotations;

namespace BluebirdCore.DTOs
{
    // Secondary Subject Assignment DTOs
    public class AssignSecondarySubjectDto
    {
        [Required]
        public int SubjectId { get; set; }
        
        public string? Notes { get; set; }
        public string? AssignedBy { get; set; }
    }

    public class StudentSubjectDto
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public DateTime EnrolledDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public string? AssignedBy { get; set; }
    }

    public class SecondaryStudentWithSubjectsDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public int SubjectCount { get; set; }
        public List<StudentSubjectDto> Subjects { get; set; } = new List<StudentSubjectDto>();
        public DateTime LastUpdated { get; set; }
    }

    public class BulkAssignSecondarySubjectsDto
    {
        [Required]
        public List<int> StudentIds { get; set; } = new List<int>();
        
        [Required]
        public List<int> SubjectIds { get; set; } = new List<int>();
        
        public string? Notes { get; set; }
        public string? AssignedBy { get; set; }
    }
}
