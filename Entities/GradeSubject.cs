namespace SchoolErpSMS.Entities
{
    public class GradeSubject
    {
        public int Id { get; set; }
        public int GradeId { get; set; }
        public int SubjectId { get; set; }
        public bool IsOptional { get; set; } = false;
        public bool IsActive { get; set; } = true;
        
        // Auto-assignment fields
        public bool AutoAssignToStudents { get; set; } = false;
        public int? AcademicYearId { get; set; }
        
        // Navigation properties
        public virtual Grade? Grade { get; set; }
        public virtual Subject? Subject { get; set; }
        public virtual AcademicYear? AcademicYear { get; set; }
    }
}