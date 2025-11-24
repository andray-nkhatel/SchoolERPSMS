using System.ComponentModel.DataAnnotations;

namespace SchoolErpSMS.Entities
{
    public class BabyClassSkill
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public required string Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int Order { get; set; } // For sorting skills in report card
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<BabyClassSkillItem> SkillItems { get; set; } = new List<BabyClassSkillItem>();
    }
}
