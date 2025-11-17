using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace BluebirdCore.Entities
{

    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [StringLength(10)]
        public string? Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        [JsonIgnore]
        public virtual ICollection<GradeSubject> GradeSubjects { get; set; } = new List<GradeSubject>();
        [JsonIgnore]
        public virtual ICollection<TeacherSubjectAssignment> TeacherAssignments { get; set; } = new List<TeacherSubjectAssignment>();
        [JsonIgnore]
        public virtual ICollection<ExamScore> ExamScores { get; set; } = new List<ExamScore>();
        [JsonIgnore]
        public virtual ICollection<StudentOptionalSubject> StudentOptionalSubjects { get; set; } = new List<StudentOptionalSubject>();
        [JsonIgnore]
        public virtual ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
    }
}