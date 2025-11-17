using System.ComponentModel.DataAnnotations;

namespace BluebirdCore.Entities
{
    public class BabyClassSkillItem
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public required string Name { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public int SkillId { get; set; } // Foreign key to BabyClassSkill
        public int Order { get; set; } // For sorting items within a skill
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual BabyClassSkill Skill { get; set; }
        public virtual ICollection<BabyClassSkillAssessment> SkillAssessments { get; set; } = new List<BabyClassSkillAssessment>();
    }
}