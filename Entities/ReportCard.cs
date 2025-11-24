using System.ComponentModel.DataAnnotations;

namespace SchoolErpSMS.Entities
{
     public class ReportCard
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int GradeId { get; set; }
        public int AcademicYear { get; set; }
        public int Term { get; set; }
        
        /// <summary>
        /// The PDF content of the report card, stored as a byte array in the database.
        /// </summary>
        // public byte[] PdfContent { get; set; }
        
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public int GeneratedBy { get; set; }
        
        // General Comment fields for manual teacher comments
        [StringLength(2000)]
        public string? GeneralComment { get; set; }
        
        public DateTime? GeneralCommentUpdatedAt { get; set; }
        public int? GeneralCommentUpdatedBy { get; set; }
        
        // Navigation properties
        public virtual Student? Student { get; set; }
        public virtual Grade? Grade { get; set; }
        public virtual User? GeneratedByUser { get; set; }
        public virtual User? GeneralCommentUpdatedByUser { get; set; }
    }
}