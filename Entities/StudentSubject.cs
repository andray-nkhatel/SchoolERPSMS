using System.ComponentModel.DataAnnotations;

namespace SchoolErpSMS.Entities
{
    public enum SubjectAssignmentSource
    {
        Manual = 0,      // Manually assigned by admin/teacher
        Inherited = 1,   // Inherited from grade assignment
        Custom = 2       // Custom assignment (not inherited)
    }

    public class StudentSubject
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        
        // Simple assignment details
        public DateTime EnrolledDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDate { get; set; }
        public DateTime? DroppedDate { get; set; }
        
        // Status
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }
        
        // Assignment metadata
        public string? AssignedBy { get; set; }
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;
        
        // Inheritance tracking
        public SubjectAssignmentSource SourceType { get; set; } = SubjectAssignmentSource.Manual;
        public int? InheritedFromGradeId { get; set; }
        
        // Navigation properties
        public virtual Student? Student { get; set; }
        public virtual Subject? Subject { get; set; }
        public virtual Grade? InheritedFromGrade { get; set; }
    }
}
