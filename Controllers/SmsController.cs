using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BluebirdCore.Services;
using BluebirdCore.Data;
using BluebirdCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BluebirdCore.Controllers
{
    /// <summary>
    /// SMS service endpoints for sending SMS messages via Zamtel Bulk SMS API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SmsController : ControllerBase
    {
        private readonly ISmsService _smsService;
        private readonly ILogger<SmsController> _logger;
        private readonly SchoolDbContext _context;

        public SmsController(ISmsService smsService, ILogger<SmsController> logger, SchoolDbContext context)
        {
            _smsService = smsService;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Send SMS to a single phone number
        /// </summary>
        /// <remarks>
        /// Sends an SMS message to a single recipient using the Zamtel Bulk SMS API.
        /// 
        /// **Phone Number Formats:**
        /// - With country code: `260950003929`
        /// - Local format: `0950003929` (will automatically add country code 260)
        /// - Local format: `950003929` (will automatically add country code 260)
        /// 
        /// **Example Request:**
        /// ```json
        /// {
        ///   "phoneNumber": "260950003929",
        ///   "message": "Hello! This is a test message from the School Management System."
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">SMS request containing phone number and message</param>
        /// <returns>Success response with phone number, or error response</returns>
        /// <response code="200">SMS sent successfully</response>
        /// <response code="400">Invalid request (missing phone number or message)</response>
        /// <response code="401">Unauthorized (missing or invalid JWT token)</response>
        /// <response code="403">Forbidden (insufficient permissions - requires Admin or Staff role)</response>
        /// <response code="500">SMS service error (check logs for details)</response>
        [HttpPost("send")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(SmsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SmsResponseDto>> SendSms([FromBody] SendSmsDto request)
        {
            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return BadRequest(new ErrorResponseDto { Message = "Phone number is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ErrorResponseDto { Message = "Message is required" });
            }

            try
            {
                var result = await _smsService.SendSmsAsync(request.PhoneNumber, request.Message);
                
                if (result)
                {
                    return Ok(new SmsResponseDto 
                    { 
                        Success = true,
                        Message = "SMS sent successfully", 
                        PhoneNumber = request.PhoneNumber 
                    });
                }
                else
                {
                    return StatusCode(500, new ErrorResponseDto 
                    { 
                        Message = "Failed to send SMS. Please check your SMS API configuration and logs." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending SMS");
                return StatusCode(500, new ErrorResponseDto 
                { 
                    Message = "An error occurred while sending SMS", 
                    Error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Send bulk SMS to multiple phone numbers
        /// </summary>
        /// <remarks>
        /// Sends the same SMS message to multiple recipients using the Zamtel Bulk SMS API.
        /// Messages are sent sequentially with a small delay between each to avoid rate limiting.
        /// 
        /// **Note:** This endpoint requires Admin role.
        /// 
        /// **Example Request:**
        /// ```json
        /// {
        ///   "phoneNumbers": [
        ///     "260950003929",
        ///     "260950003930",
        ///     "0950003929"
        ///   ],
        ///   "message": "Important announcement: School will be closed tomorrow."
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Bulk SMS request containing list of phone numbers and message</param>
        /// <returns>Success response with count of messages sent, or error response</returns>
        /// <response code="200">All SMS messages sent successfully</response>
        /// <response code="400">Invalid request (missing phone numbers or message)</response>
        /// <response code="401">Unauthorized (missing or invalid JWT token)</response>
        /// <response code="403">Forbidden (insufficient permissions - requires Admin role)</response>
        /// <response code="500">Some or all SMS messages failed to send</response>
        [HttpPost("send/bulk")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(BulkSmsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BulkSmsResponseDto>> SendBulkSms([FromBody] SendBulkSmsDto request)
        {
            if (request.PhoneNumbers == null || !request.PhoneNumbers.Any())
            {
                return BadRequest(new ErrorResponseDto { Message = "At least one phone number is required" });
            }

            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ErrorResponseDto { Message = "Message is required" });
            }

            try
            {
                var result = await _smsService.SendBulkSmsAsync(request.PhoneNumbers, request.Message);
                
                if (result)
                {
                    return Ok(new BulkSmsResponseDto 
                    { 
                        Success = true,
                        Message = "All SMS messages sent successfully", 
                        Count = request.PhoneNumbers.Count 
                    });
                }
                else
                {
                    return StatusCode(500, new ErrorResponseDto 
                    { 
                        Message = "Some SMS messages failed to send. Please check logs for details." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk SMS");
                return StatusCode(500, new ErrorResponseDto 
                { 
                    Message = "An error occurred while sending bulk SMS", 
                    Error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Send student marks via SMS for a specific term (includes all three exam types: Test1, Test2, End-of-Term)
        /// </summary>
        /// <remarks>
        /// Retrieves a student's marks for all three exam types (Test1, Test2, End-of-Term) for a given term,
        /// formats them into a concise SMS message, and sends it to the specified phone number (or student's phone if not provided).
        /// 
        /// **Example Request:**
        /// ```json
        /// {
        ///   "studentId": 1,
        ///   "term": 2,
        ///   "academicYear": 2024,
        ///   "phoneNumber": "260950003929"
        /// }
        /// ```
        /// 
        /// **Example SMS Output:**
        /// "John Mbuki, Form 1 W - Term 2 (2024): Math T1:78 T2:82 ET:85, Eng T1:75 T2:80 ET:82, Sci T1:70 T2:75 ET:78. Full report at school."
        /// </remarks>
        /// <param name="request">Request containing student ID, term, and optional phone number</param>
        /// <returns>Success response with SMS details, or error response</returns>
        /// <response code="200">SMS sent successfully with student marks</response>
        /// <response code="400">Invalid request (missing required fields or no marks found)</response>
        /// <response code="404">Student not found</response>
        /// <response code="401">Unauthorized (missing or invalid JWT token)</response>
        /// <response code="403">Forbidden (insufficient permissions - requires Admin or Staff role)</response>
        /// <response code="500">SMS service error (check logs for details)</response>
        [HttpPost("send/student-marks")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(StudentMarksSmsResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<StudentMarksSmsResponseDto>> SendStudentMarksSms([FromBody] SendStudentMarksSmsDto request)
        {
            if (request.StudentId <= 0)
            {
                return BadRequest(new ErrorResponseDto { Message = "Valid student ID is required" });
            }

            if (request.Term < 1 || request.Term > 3)
            {
                return BadRequest(new ErrorResponseDto { Message = "Term must be 1, 2, or 3" });
            }

            try
            {
                // Get student
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == request.StudentId);

                if (student == null)
                {
                    return NotFound(new ErrorResponseDto { Message = "Student not found" });
                }

                // Determine phone number (use provided or student's phone)
                var phoneNumber = !string.IsNullOrWhiteSpace(request.PhoneNumber) 
                    ? request.PhoneNumber 
                    : student.GuardianPhone ?? student.PhoneNumber;

                if (string.IsNullOrWhiteSpace(phoneNumber))
                {
                    return BadRequest(new ErrorResponseDto 
                    { 
                        Message = "Phone number is required. Please provide a phone number or ensure the student has a phone number or guardian phone number." 
                    });
                }

                // Determine academic year ID (AcademicYear field stores the ID, not the year value)
                int academicYearId;
                if (request.AcademicYear.HasValue)
                {
                    // Check if the provided value is an AcademicYear ID
                    var academicYearEntity = await _context.AcademicYears
                        .FirstOrDefaultAsync(ay => ay.Id == request.AcademicYear.Value);
                    
                    if (academicYearEntity != null)
                    {
                        academicYearId = request.AcademicYear.Value;
                    }
                    else
                    {
                        // If not found as ID, try to find by year value in Name field (e.g., "2025")
                        var yearByName = await _context.AcademicYears
                            .FirstOrDefaultAsync(ay => ay.Name.Contains(request.AcademicYear.Value.ToString()));
                        
                        if (yearByName != null)
                        {
                            academicYearId = yearByName.Id;
                        }
                        else
                        {
                            // Default to active academic year if provided value doesn't match
                            var activeYear = await _context.AcademicYears
                                .FirstOrDefaultAsync(ay => ay.IsActive);
                            academicYearId = activeYear?.Id ?? 1; // Fallback to ID 1 if no active year
                        }
                    }
                }
                else
                {
                    // Get active academic year ID
                    var activeYear = await _context.AcademicYears
                        .FirstOrDefaultAsync(ay => ay.IsActive);
                    academicYearId = activeYear?.Id ?? 1; // Fallback to ID 1 if no active year
                }

                // Get student with grade information
                var studentWithGrade = await _context.Students
                    .Include(s => s.Grade)
                    .FirstOrDefaultAsync(s => s.Id == request.StudentId);

                if (studentWithGrade == null)
                {
                    return NotFound(new ErrorResponseDto { Message = "Student not found" });
                }

                // Get all three exam types (Test-One, Test-Two, End-of-Term)
                // Match exact names from database: "Test-One", "Test-Two", "End-of-Term"
                var examTypes = await _context.ExamTypes
                    .Where(et => et.Name == "Test-One" || 
                                 et.Name == "Test-Two" || 
                                 et.Name == "End-of-Term")
                    .ToListAsync();

                if (!examTypes.Any())
                {
                    // Get available exam types for better error message
                    var availableTypes = await _context.ExamTypes.Select(et => et.Name).ToListAsync();
                    return BadRequest(new ErrorResponseDto 
                    { 
                        Message = $"No matching exam types found. Required: Test-One, Test-Two, End-of-Term. Available exam types: {string.Join(", ", availableTypes)}" 
                    });
                }

                _logger.LogInformation($"Found {examTypes.Count} exam types: {string.Join(", ", examTypes.Select(et => et.Name))}");

                // Get student marks for the term and all three exam types
                var examTypeIds = examTypes.Select(et => et.Id).ToList();
                var marks = await _context.ExamScores
                    .Include(es => es.Subject)
                    .Include(es => es.ExamType)
                    .Where(es => es.StudentId == request.StudentId &&
                                es.AcademicYear == academicYearId &&
                                es.Term == request.Term &&
                                examTypeIds.Contains(es.ExamTypeId))
                    .OrderBy(es => es.Subject.Name)
                    .ThenBy(es => es.ExamType.Order)
                    .ToListAsync();

                if (!marks.Any())
                {
                    // Check if student has any marks for this term/year (for better error message)
                    var anyMarksForTerm = await _context.ExamScores
                        .AnyAsync(es => es.StudentId == request.StudentId &&
                                      es.AcademicYear == academicYearId &&
                                      es.Term == request.Term);

                    if (!anyMarksForTerm)
                    {
                        // Check what terms/years the student has marks for
                        var availableTerms = await _context.ExamScores
                            .Where(es => es.StudentId == request.StudentId)
                            .Select(es => new { es.AcademicYear, es.Term })
                            .Distinct()
                            .OrderByDescending(x => x.AcademicYear)
                            .ThenByDescending(x => x.Term)
                            .Take(5)
                            .ToListAsync();

                        // Get academic year names for better display
                        var academicYearIds = availableTerms.Select(t => t.AcademicYear).Distinct().ToList();
                        var academicYearNames = await _context.AcademicYears
                            .Where(ay => academicYearIds.Contains(ay.Id))
                            .ToDictionaryAsync(ay => ay.Id, ay => ay.Name);

                        var availableInfo = availableTerms.Any()
                            ? $" Available marks found for: {string.Join(", ", availableTerms.Select(t => $"Year {academicYearNames.GetValueOrDefault(t.AcademicYear, t.AcademicYear.ToString())} Term {t.Term}"))}"
                            : " No marks found for this student in any term or year.";

                        var requestedYearName = await _context.AcademicYears
                            .Where(ay => ay.Id == academicYearId)
                            .Select(ay => ay.Name)
                            .FirstOrDefaultAsync() ?? academicYearId.ToString();

                        return BadRequest(new ErrorResponseDto 
                        { 
                            Message = $"No marks found for student {studentWithGrade.FullName} for Term {request.Term}, Academic Year {requestedYearName}.{availableInfo}" 
                        });
                    }
                    else
                    {
                        // Student has marks but not for the requested exam types
                        var availableExamTypes = await _context.ExamScores
                            .Include(es => es.ExamType)
                            .Where(es => es.StudentId == request.StudentId &&
                                        es.AcademicYear == academicYearId &&
                                        es.Term == request.Term)
                            .Select(es => es.ExamType.Name)
                            .Distinct()
                            .ToListAsync();

                        var requestedYearName2 = await _context.AcademicYears
                            .Where(ay => ay.Id == academicYearId)
                            .Select(ay => ay.Name)
                            .FirstOrDefaultAsync() ?? academicYearId.ToString();

                        return BadRequest(new ErrorResponseDto 
                        { 
                            Message = $"No marks found for student {studentWithGrade.FullName} for Term {request.Term}, Academic Year {requestedYearName2} with exam types: Test-One, Test-Two, End-of-Term. Available exam types for this term: {string.Join(", ", availableExamTypes)}" 
                        });
                    }
                }

                // Get academic year name for display
                var yearEntity = await _context.AcademicYears.FindAsync(academicYearId);
                var academicYearName = yearEntity?.Name ?? academicYearId.ToString();
                var academicYearForDisplay = yearEntity != null && int.TryParse(yearEntity.Name, out var yearValue) 
                    ? yearValue 
                    : academicYearId;

                _logger.LogInformation($"Found {marks.Count} marks for student {studentWithGrade.FullName}, Term {request.Term}, Academic Year ID {academicYearId} ({academicYearName})");

                // Format the SMS message with all three exam types
                var message = FormatAllExamTypesMessage(
                    studentWithGrade.FullName,
                    studentWithGrade.Grade?.FullName ?? studentWithGrade.Grade?.Name ?? "Unknown Class",
                    request.Term,
                    academicYearForDisplay,
                    marks
                );

                // Send SMS
                var result = await _smsService.SendSmsAsync(phoneNumber, message);

                if (result)
                {
                    // Calculate totals for each exam type
                    var test1Marks = marks.Where(m => m.ExamType.Name.Contains("Test-One") || m.ExamType.Name.Contains("Test1"));
                    var test2Marks = marks.Where(m => m.ExamType.Name.Contains("Test-Two") || m.ExamType.Name.Contains("Mid-Term") || m.ExamType.Name.Contains("Test2"));
                    var endTermMarks = marks.Where(m => m.ExamType.Name.Contains("End-of-Term") || m.ExamType.Name.Contains("End-Term"));

                    return Ok(new StudentMarksSmsResponseDto
                    {
                        Success = true,
                        Message = "Student marks SMS sent successfully",
                        PhoneNumber = phoneNumber,
                        StudentName = student.FullName,
                        Term = request.Term,
                        ExamType = "All (Test1, Test2, End-of-Term)",
                        AcademicYear = academicYearForDisplay,
                        MarksCount = marks.Count,
                        TotalScore = marks.Where(m => !m.IsAbsent).Sum(m => m.Score)
                    });
                }
                else
                {
                    return StatusCode(500, new ErrorResponseDto 
                    { 
                        Message = "Failed to send SMS. Please check your SMS API configuration and logs." 
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending student marks SMS");
                return StatusCode(500, new ErrorResponseDto 
                { 
                    Message = "An error occurred while sending student marks SMS", 
                    Error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Get preview of student marks SMS message without sending
        /// </summary>
        /// <remarks>
        /// Retrieves a student's marks for all three exam types (Test1, Test2, End-of-Term) for a given term,
        /// formats them into a concise SMS message, and returns the message content without sending it.
        /// 
        /// **Example Request:**
        /// ```json
        /// {
        ///   "studentId": 1,
        ///   "term": 2,
        ///   "academicYear": 2024
        /// }
        /// ```
        /// </remarks>
        /// <param name="request">Request containing student ID, term, and optional academic year</param>
        /// <returns>Preview response with message content, or error response</returns>
        /// <response code="200">Message preview generated successfully</response>
        /// <response code="400">Invalid request (missing required fields or no marks found)</response>
        /// <response code="404">Student not found</response>
        /// <response code="401">Unauthorized (missing or invalid JWT token)</response>
        /// <response code="403">Forbidden (insufficient permissions - requires Admin or Staff role)</response>
        [HttpPost("preview/student-marks")]
        [Authorize(Roles = "Admin,Staff")]
        [ProducesResponseType(typeof(StudentMarksSmsPreviewDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<StudentMarksSmsPreviewDto>> PreviewStudentMarksSms([FromBody] SendStudentMarksSmsDto request)
        {
            if (request.StudentId <= 0)
            {
                return BadRequest(new ErrorResponseDto { Message = "Valid student ID is required" });
            }

            if (request.Term < 1 || request.Term > 3)
            {
                return BadRequest(new ErrorResponseDto { Message = "Term must be 1, 2, or 3" });
            }

            try
            {
                // Get student
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.Id == request.StudentId);

                if (student == null)
                {
                    return NotFound(new ErrorResponseDto { Message = "Student not found" });
                }

                // Determine academic year ID (same logic as SendStudentMarksSms)
                int academicYearId;
                if (request.AcademicYear.HasValue)
                {
                    var academicYearEntity = await _context.AcademicYears
                        .FirstOrDefaultAsync(ay => ay.Id == request.AcademicYear.Value);
                    
                    if (academicYearEntity != null)
                    {
                        academicYearId = request.AcademicYear.Value;
                    }
                    else
                    {
                        var yearByName = await _context.AcademicYears
                            .FirstOrDefaultAsync(ay => ay.Name.Contains(request.AcademicYear.Value.ToString()));
                        
                        if (yearByName != null)
                        {
                            academicYearId = yearByName.Id;
                        }
                        else
                        {
                            var activeYear = await _context.AcademicYears
                                .FirstOrDefaultAsync(ay => ay.IsActive);
                            academicYearId = activeYear?.Id ?? 1;
                        }
                    }
                }
                else
                {
                    var activeYear = await _context.AcademicYears
                        .FirstOrDefaultAsync(ay => ay.IsActive);
                    academicYearId = activeYear?.Id ?? 1;
                }

                // Get student with grade information
                var studentWithGrade = await _context.Students
                    .Include(s => s.Grade)
                    .FirstOrDefaultAsync(s => s.Id == request.StudentId);

                if (studentWithGrade == null)
                {
                    return NotFound(new ErrorResponseDto { Message = "Student not found" });
                }

                // Get all three exam types
                var examTypes = await _context.ExamTypes
                    .Where(et => et.Name == "Test-One" || 
                                 et.Name == "Test-Two" || 
                                 et.Name == "End-of-Term")
                    .ToListAsync();

                if (!examTypes.Any())
                {
                    var availableTypes = await _context.ExamTypes.Select(et => et.Name).ToListAsync();
                    return BadRequest(new ErrorResponseDto 
                    { 
                        Message = $"No matching exam types found. Required: Test-One, Test-Two, End-of-Term. Available exam types: {string.Join(", ", availableTypes)}" 
                    });
                }

                // Get student marks for the term and all three exam types
                var examTypeIds = examTypes.Select(et => et.Id).ToList();
                var marks = await _context.ExamScores
                    .Include(es => es.Subject)
                    .Include(es => es.ExamType)
                    .Where(es => es.StudentId == request.StudentId &&
                                es.AcademicYear == academicYearId &&
                                es.Term == request.Term &&
                                examTypeIds.Contains(es.ExamTypeId))
                    .OrderBy(es => es.Subject.Name)
                    .ThenBy(es => es.ExamType.Order)
                    .ToListAsync();

                if (!marks.Any())
                {
                    var requestedYearName = await _context.AcademicYears
                        .Where(ay => ay.Id == academicYearId)
                        .Select(ay => ay.Name)
                        .FirstOrDefaultAsync() ?? academicYearId.ToString();

                    return BadRequest(new ErrorResponseDto 
                    { 
                        Message = $"No marks found for student {studentWithGrade.FullName} for Term {request.Term}, Academic Year {requestedYearName}." 
                    });
                }

                // Get academic year name for display
                var yearEntity = await _context.AcademicYears.FindAsync(academicYearId);
                var academicYearName = yearEntity?.Name ?? academicYearId.ToString();
                var academicYearForDisplay = yearEntity != null && int.TryParse(yearEntity.Name, out var yearValue) 
                    ? yearValue 
                    : academicYearId;

                // Format the SMS message with all three exam types (same method as SendStudentMarksSms)
                var message = FormatAllExamTypesMessage(
                    studentWithGrade.FullName,
                    studentWithGrade.Grade?.FullName ?? studentWithGrade.Grade?.Name ?? "Unknown Class",
                    request.Term,
                    academicYearForDisplay,
                    marks
                );

                // Determine phone number (use provided or student's phone)
                var phoneNumber = !string.IsNullOrWhiteSpace(request.PhoneNumber) 
                    ? request.PhoneNumber 
                    : student.GuardianPhone ?? student.PhoneNumber ?? "N/A";

                return Ok(new StudentMarksSmsPreviewDto
                {
                    Message = message,
                    Preview = message,
                    Content = message,
                    StudentName = student.FullName,
                    Term = request.Term,
                    AcademicYear = academicYearForDisplay,
                    PhoneNumber = phoneNumber,
                    MarksCount = marks.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating student marks SMS preview");
                return StatusCode(500, new ErrorResponseDto 
                { 
                    Message = "An error occurred while generating SMS preview", 
                    Error = ex.Message 
                });
            }
        }

        /// <summary>
        /// Formats student marks into an SMS message with key information from ReportCardPdfService
        /// </summary>
        /// <param name="studentName">Student's full name (highly required)</param>
        /// <param name="className">Student's class/grade (highly required)</param>
        /// <param name="term">Term number (1, 2, or 3)</param>
        /// <param name="examTypeName">Exam type name (e.g., "End-of-Term")</param>
        /// <param name="academicYear">Academic year</param>
        /// <param name="marks">List of exam scores</param>
        /// <returns>Formatted SMS message</returns>
        private string FormatMarksMessage(
            string studentName, 
            string className, 
            int term, 
            string examTypeName, 
            int academicYear, 
            List<ExamScore> marks)
        {
            // Get subject abbreviation mapping
            string GetSubjectAbbreviation(string subjectName)
            {
                var name = subjectName.ToLower();
                return name switch
                {
                    var n when n.Contains("mathematics") || n.Contains("math") => "Math",
                    var n when n.Contains("english") => "Eng",
                    var n when n.Contains("science") => "Sci",
                    var n when n.Contains("social") && n.Contains("studies") => "SST",
                    var n when n.Contains("social") && n.Contains("science") => "SST",
                    var n when n.Contains("history") => "Hist",
                    var n when n.Contains("geography") => "Geo",
                    var n when n.Contains("physics") => "Phy",
                    var n when n.Contains("chemistry") => "Chem",
                    var n when n.Contains("biology") => "Bio",
                    var n when n.Contains("physical") && n.Contains("education") => "PE",
                    var n when n.Contains("religious") || n.Contains("re") => "RE",
                    var n when n.Contains("computer") || n.Contains("ict") => "ICT",
                    var n when n.Contains("agriculture") || n.Contains("agric") => "Agric",
                    var n when n.Contains("business") => "Bus",
                    var n when n.Contains("accounts") || n.Contains("accounting") => "Acc",
                    var n when n.Contains("literature") => "Lit",
                    var n when n.Contains("french") => "Fr",
                    var n when n.Contains("art") => "Art",
                    var n when n.Contains("music") => "Music",
                    _ => subjectName.Length > 8 ? subjectName.Substring(0, 8) : subjectName // Truncate if too long
                };
            }

            // Format exam type name (e.g., "End-of-Term" -> "End-Term")
            var examTypeShort = examTypeName.Replace("of-", "").Replace("of ", "");

            // Build marks list
            var marksList = new List<string>();
            decimal total = 0;
            int presentCount = 0;

            foreach (var mark in marks)
            {
                if (mark.IsAbsent)
                {
                    marksList.Add($"{GetSubjectAbbreviation(mark.Subject.Name)} Abs");
                }
                else
                {
                    var score = (int)Math.Round(mark.Score);
                    marksList.Add($"{GetSubjectAbbreviation(mark.Subject.Name)} {score}");
                    total += mark.Score;
                    presentCount++;
                }
            }

            var marksText = string.Join(", ", marksList);
            var totalInt = (int)Math.Round(total);
            
            // Calculate average (if we have present marks)
            var average = presentCount > 0 ? total / presentCount : 0;
            var averageInt = (int)Math.Round(average);

            // Format: Student Name, Class - Term X ExamType (Year): Scores. Total: X, Avg: X. Full report at school.
            // Example: "John Mbuki, Form 1 W - Term 3 End-Term (2025): Math 78, Eng 82, Sci 74, SST 69. Total: 303, Avg: 76. Full report at school."
            return $"{studentName}, {className} - Term {term} {examTypeShort} ({academicYear}): {marksText}. Total: {totalInt}, Avg: {averageInt}. Full report at school.";
        }

        /// <summary>
        /// Formats student marks into an SMS message with all three exam types (Test1, Test2, End-of-Term)
        /// Uses concise wording but includes all results
        /// </summary>
        /// <param name="studentName">Student's full name</param>
        /// <param name="className">Student's class/grade</param>
        /// <param name="term">Term number (1, 2, or 3)</param>
        /// <param name="academicYear">Academic year</param>
        /// <param name="allMarks">List of exam scores for all three exam types</param>
        /// <returns>Formatted SMS message with all three exam types</returns>
        private string FormatAllExamTypesMessage(
            string studentName, 
            string className, 
            int term, 
            int academicYear, 
            List<ExamScore> allMarks)
        {
            // Get subject abbreviation mapping
            string GetSubjectAbbreviation(string subjectName)
            {
                var name = subjectName.ToLower();
                return name switch
                {
                    var n when n.Contains("mathematics") || n.Contains("math") => "Math",
                    var n when n.Contains("english") => "Eng",
                    var n when n.Contains("science") => "Sci",
                    var n when n.Contains("social") && n.Contains("studies") => "SST",
                    var n when n.Contains("social") && n.Contains("science") => "SST",
                    var n when n.Contains("history") => "Hist",
                    var n when n.Contains("geography") => "Geo",
                    var n when n.Contains("physics") => "Phy",
                    var n when n.Contains("chemistry") => "Chem",
                    var n when n.Contains("biology") => "Bio",
                    var n when n.Contains("physical") && n.Contains("education") => "PE",
                    var n when n.Contains("religious") || n.Contains("re") => "RE",
                    var n when n.Contains("computer") || n.Contains("ict") => "ICT",
                    var n when n.Contains("agriculture") || n.Contains("agric") => "Agric",
                    var n when n.Contains("business") => "Bus",
                    var n when n.Contains("accounts") || n.Contains("accounting") => "Acc",
                    var n when n.Contains("literature") => "Lit",
                    var n when n.Contains("french") => "Fr",
                    var n when n.Contains("art") => "Art",
                    var n when n.Contains("music") => "Music",
                    _ => subjectName.Length > 8 ? subjectName.Substring(0, 8) : subjectName
                };
            }

            // Helper to identify exam type
            bool IsTest1(ExamType examType)
            {
                var name = examType.Name.ToLower();
                return name.Contains("test-one") || name.Contains("test1") || name.Contains("t1");
            }

            bool IsTest2(ExamType examType)
            {
                var name = examType.Name.ToLower();
                return name.Contains("test-two") || name.Contains("test2") || name.Contains("t2") || 
                       name.Contains("mid-term") || name.Contains("midterm");
            }

            bool IsEndTerm(ExamType examType)
            {
                var name = examType.Name.ToLower();
                return name.Contains("end-of-term") || name.Contains("end-term") || name.Contains("endterm");
            }

            // Group marks by subject
            var bySubject = allMarks
                .GroupBy(m => m.Subject)
                .OrderBy(g => g.Key.Name)
                .ToList();

            var subjectScores = new List<string>();

            foreach (var subjectGroup in bySubject)
            {
                var subject = subjectGroup.Key;
                var subjectAbbr = GetSubjectAbbreviation(subject.Name);
                
                // Get marks for each exam type
                var test1 = subjectGroup.FirstOrDefault(m => IsTest1(m.ExamType));
                var test2 = subjectGroup.FirstOrDefault(m => IsTest2(m.ExamType));
                var endTerm = subjectGroup.FirstOrDefault(m => IsEndTerm(m.ExamType));

                // Build scores string - always include all three, use "N/A" if missing
                var scores = new List<string>();
                
                if (test1 != null)
                {
                    scores.Add($"T1:{(test1.IsAbsent ? "Abs" : ((int)Math.Round(test1.Score)).ToString())}");
                }
                else
                {
                    scores.Add("T1:N/A");
                }

                if (test2 != null)
                {
                    scores.Add($"T2:{(test2.IsAbsent ? "Abs" : ((int)Math.Round(test2.Score)).ToString())}");
                }
                else
                {
                    scores.Add("T2:N/A");
                }

                if (endTerm != null)
                {
                    scores.Add($"ET:{(endTerm.IsAbsent ? "Abs" : ((int)Math.Round(endTerm.Score)).ToString())}");
                }
                else
                {
                    scores.Add("ET:N/A");
                }

                // Format: "Math T1:78 T2:82 ET:85"
                subjectScores.Add($"{subjectAbbr} {string.Join(" ", scores)}");
            }

            // Build the message
            var scoresText = string.Join(", ", subjectScores);
            var message = $"{studentName}, {className} - Term {term} ({academicYear}): {scoresText}. Full report at school.";

            // Ensure message fits within SMS limit (160 chars)
            // If too long, we'll need to truncate or use a more compact format
            if (message.Length > 160)
            {
                // Try to make it more compact by shortening the ending
                var compactEnding = ". Full report at school.";
                var maxSubjectLength = 160 - (studentName.Length + className.Length + 20 + compactEnding.Length);
                
                if (maxSubjectLength > 50)
                {
                    // Truncate subject list if needed
                    var truncatedScores = new List<string>();
                    var currentLength = studentName.Length + className.Length + 20 + compactEnding.Length;
                    
                    foreach (var score in subjectScores)
                    {
                        if (currentLength + score.Length + 2 > 160) // +2 for ", "
                        {
                            break;
                        }
                        truncatedScores.Add(score);
                        currentLength += score.Length + 2;
                    }
                    
                    if (truncatedScores.Any())
                    {
                        message = $"{studentName}, {className} - Term {term} ({academicYear}): {string.Join(", ", truncatedScores)}. Full report at school.";
                    }
                }
            }

            return message;
        }
    }

    /// <summary>
    /// Request DTO for sending a single SMS
    /// </summary>
    public class SendSmsDto
    {
        /// <summary>
        /// Phone number in any format (with or without country code)
        /// Examples: "260950003929", "0950003929", "950003929"
        /// </summary>
        /// <example>260950003929</example>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// SMS message content
        /// </summary>
        /// <example>Hello! This is a test message from the School Management System.</example>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request DTO for sending bulk SMS
    /// </summary>
    public class SendBulkSmsDto
    {
        /// <summary>
        /// List of phone numbers to send SMS to
        /// </summary>
        /// <example>["260950003929", "260950003930", "0950003929"]</example>
        public List<string> PhoneNumbers { get; set; } = new();

        /// <summary>
        /// SMS message content (same message sent to all recipients)
        /// </summary>
        /// <example>Important announcement: School will be closed tomorrow.</example>
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO for single SMS
    /// </summary>
    public class SmsResponseDto
    {
        /// <summary>
        /// Indicates if the SMS was sent successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Phone number the SMS was sent to
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response DTO for bulk SMS
    /// </summary>
    public class BulkSmsResponseDto
    {
        /// <summary>
        /// Indicates if all SMS messages were sent successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Number of SMS messages sent
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// Error response DTO
    /// </summary>
    public class ErrorResponseDto
    {
        /// <summary>
        /// Error message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Detailed error information (if available)
        /// </summary>
        public string? Error { get; set; }
    }

    /// <summary>
    /// Request DTO for sending student marks via SMS
    /// </summary>
    public class SendStudentMarksSmsDto
    {
        /// <summary>
        /// Student ID
        /// </summary>
        /// <example>1</example>
        public int StudentId { get; set; }

        /// <summary>
        /// Term number (1, 2, or 3)
        /// </summary>
        /// <example>2</example>
        public int Term { get; set; }

        /// <summary>
        /// Academic year (optional, defaults to current year)
        /// </summary>
        /// <example>2024</example>
        public int? AcademicYear { get; set; }

        /// <summary>
        /// Phone number to send SMS to (optional, uses student's phone or guardian phone if not provided)
        /// </summary>
        /// <example>260950003929</example>
        public string? PhoneNumber { get; set; }
    }

    /// <summary>
    /// Response DTO for student marks SMS
    /// </summary>
    public class StudentMarksSmsResponseDto
    {
        /// <summary>
        /// Indicates if the SMS was sent successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Response message
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Phone number the SMS was sent to
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Student name
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Term number
        /// </summary>
        public int Term { get; set; }

        /// <summary>
        /// Exam type name
        /// </summary>
        public string ExamType { get; set; } = string.Empty;

        /// <summary>
        /// Academic year
        /// </summary>
        public int AcademicYear { get; set; }

        /// <summary>
        /// Number of marks included
        /// </summary>
        public int MarksCount { get; set; }

        /// <summary>
        /// Total score
        /// </summary>
        public decimal TotalScore { get; set; }
    }

    /// <summary>
    /// Response DTO for student marks SMS preview
    /// </summary>
    public class StudentMarksSmsPreviewDto
    {
        /// <summary>
        /// The formatted SMS message content
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Preview of the message (same as Message)
        /// </summary>
        public string Preview { get; set; } = string.Empty;

        /// <summary>
        /// Content of the message (same as Message)
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Student name
        /// </summary>
        public string StudentName { get; set; } = string.Empty;

        /// <summary>
        /// Term number
        /// </summary>
        public int Term { get; set; }

        /// <summary>
        /// Academic year
        /// </summary>
        public int AcademicYear { get; set; }

        /// <summary>
        /// Phone number that would be used for sending
        /// </summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Number of marks included
        /// </summary>
        public int MarksCount { get; set; }
    }
}


