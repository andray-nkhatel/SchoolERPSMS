using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolErpSMS.Entities
{
    /// <summary>
    /// Entity for logging SMS messages sent through the system
    /// </summary>
    public class SmsLog
    {
        public int Id { get; set; }

        /// <summary>
        /// Phone number the SMS was sent to
        /// </summary>
        [Required]
        [StringLength(20)]
        public required string PhoneNumber { get; set; }

        /// <summary>
        /// The SMS message content
        /// </summary>
        [Required]
        [StringLength(500)]
        public required string MessageContent { get; set; }

        /// <summary>
        /// Student ID (nullable, for student-related SMS)
        /// </summary>
        public int? StudentId { get; set; }

        /// <summary>
        /// User ID who sent the SMS
        /// </summary>
        public int? SentByUserId { get; set; }

        /// <summary>
        /// Timestamp when SMS was sent (stored in UTC)
        /// </summary>
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Status of the SMS: Sent, Failed, Pending
        /// </summary>
        [Required]
        [StringLength(20)]
        public required string Status { get; set; } = "Pending";

        /// <summary>
        /// Provider response (JSON string for error details or success info)
        /// </summary>
        public string? ProviderResponse { get; set; }

        /// <summary>
        /// Cost of the SMS (if applicable)
        /// </summary>
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Cost { get; set; }

        /// <summary>
        /// Type of message: StudentMarks, Bulk, Single, etc.
        /// </summary>
        [StringLength(50)]
        public string? MessageType { get; set; }

        /// <summary>
        /// Term number (for student marks SMS)
        /// </summary>
        public int? Term { get; set; }

        /// <summary>
        /// Academic year ID (for student marks SMS)
        /// </summary>
        public int? AcademicYear { get; set; }

        /// <summary>
        /// Number of retry attempts
        /// </summary>
        public int RetryCount { get; set; } = 0;

        /// <summary>
        /// Error message if sending failed
        /// </summary>
        [StringLength(500)]
        public string? ErrorMessage { get; set; }

        // Navigation properties
        public virtual Student? Student { get; set; }
        public virtual User? SentByUser { get; set; }
        public virtual AcademicYear? AcademicYearNavigation { get; set; }
    }
}

