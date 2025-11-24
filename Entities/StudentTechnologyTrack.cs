using System.ComponentModel.DataAnnotations;

namespace SchoolErpSMS.Entities
{
    public class StudentTechnologyTrack
    {
        public int Id { get; set; }
        
        public int StudentId { get; set; }
        
        [Required]
        [StringLength(50)]
        public required string TrackName { get; set; } // "Computer Science" or "Design and Technology"
        
        public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual Student? Student { get; set; }
    }
}
