using System.ComponentModel.DataAnnotations;

namespace SchoolErpSMS.Entities
{
    public class BabyClassSkillAssessment
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int SkillItemId { get; set; }
        public int AcademicYear { get; set; }
        public int Term { get; set; } // 1, 2, 3
        
        [StringLength(1000)]
        public string? TeacherComment { get; set; }
        
        public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
        public int AssessedBy { get; set; } // Teacher who made the assessment
        
        // Navigation properties
        public virtual Student Student { get; set; } = null!;
        public virtual BabyClassSkillItem SkillItem { get; set; } = null!;
        public virtual User AssessedByNavigation { get; set; } = null!;
    }
}
