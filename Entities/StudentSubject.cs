using System.ComponentModel.DataAnnotations;

namespace SchoolErpSMS.Entities
{
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
        
        // Navigation properties
        public virtual Student? Student { get; set; }
        public virtual Subject? Subject { get; set; }
    }
}
