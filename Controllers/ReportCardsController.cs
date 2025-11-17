using BluebirdCore.DTOs;
using BluebirdCore.Infrastructure;
using BluebirdCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using Hangfire;
using Microsoft.Extensions.Configuration;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

namespace BluebirdCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportCardsController : ControllerBase
    {
        private readonly IReportCardService _reportCardService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IPdfMergeService _pdfMergeService;
        private readonly ILogger<ReportCardsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IUserService _userService;

        // Constants
        private const int MinimumValidYear = 2000;
        private const string MergedPdfTempDirectory = "BluebirdCoreMergedPdfs";
        
        public ReportCardsController(
            IReportCardService reportCardService, 
            IPdfMergeService pdfMergeService,  
            IBackgroundJobClient backgroundJobClient, 
            ILogger<ReportCardsController> logger,
            IConfiguration configuration,
            IEmailService emailService,
            IUserService userService)
        {
            _reportCardService = reportCardService;
            _backgroundJobClient = backgroundJobClient;
            _pdfMergeService = pdfMergeService;
            _logger = logger;
            _configuration = configuration;
            _emailService = emailService;
            _userService = userService;
        }

        #region Endpoints

        /// <summary>
        /// Generate report card for a student (Admin only)
        /// </summary>
        [HttpPost("generate/student/{studentId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ReportCardDto>> GenerateReportCard(
            int studentId, 
            [FromQuery] int academicYear, 
            [FromQuery] int term)
        {
            var adminId = GetCurrentUserId();
            
            try
            {
                var reportCard = await _reportCardService.GenerateReportCardAsync(studentId, academicYear, term, adminId);
                return Ok(MapToDto(reportCard));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report card for student {StudentId}", studentId);
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Generate report cards for entire class (Admin only)
        /// </summary>
        [HttpPost("generate/class/{gradeId}")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<ReportCardDto>>> GenerateClassReportCards(
            int gradeId, 
            [FromQuery] int academicYear, 
            [FromQuery] int term)
        {
            var adminId = GetCurrentUserId();

            try
            {
                var reportCards = await _reportCardService.GenerateClassReportCardsAsync(gradeId, academicYear, term, adminId);
                var reportCardDtos = reportCards.Select(MapToDto);
                return Ok(reportCardDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating class report cards for grade {GradeId}", gradeId);
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Download report card PDF (Admin only)
        /// </summary>
        [HttpGet("{reportCardId}/download")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult> DownloadReportCard(int reportCardId)
        {
            try
            {
                var pdfBytes = await _reportCardService.GetReportCardPdfAsync(reportCardId);
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    _logger.LogWarning("PDF bytes are null or empty for Report Card {ReportCardId}", reportCardId);
                    return NotFound(new { message = "PDF not found for this report card." });
                }

                return File(pdfBytes, "application/pdf", $"ReportCard_{reportCardId}.pdf");
            }
            catch (InvalidOperationException ioEx)
            {
                // More specific error message for InvalidOperationException
                _logger.LogError(ioEx, "Error downloading report card {ReportCardId}: {Message}", reportCardId, ioEx.Message);
                return StatusCode(500, new { message = ioEx.Message, error = "Failed to generate or retrieve report card PDF" });
            }
            catch (ArgumentException argEx)
            {
                _logger.LogError(argEx, "Invalid argument for report card {ReportCardId}: {Message}", reportCardId, argEx.Message);
                return BadRequest(new { message = argEx.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error downloading report card {ReportCardId}. Exception: {ExceptionType}, Message: {Message}", 
                    reportCardId, ex.GetType().Name, ex.Message);
                return StatusCode(500, new { message = $"An error occurred while generating the report card PDF: {ex.Message}", error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get report card details by ID
        /// </summary>
        [HttpGet("{reportCardId}")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<ReportCardDto>> GetReportCard(int reportCardId)
        {
            try
            {
                var reportCard = await _reportCardService.GetReportCardByIdAsync(reportCardId);
                if (reportCard == null)
                {
                    return NotFound(new { message = "Report card not found." });
                }

                return Ok(MapToDto(reportCard));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report card {ReportCardId}", reportCardId);
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Get all report cards for a student
        /// </summary>
        [HttpGet("student/{studentId}")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<ReportCardDto>>> GetStudentReportCards(int studentId)
        {
            try
            {
                var reportCards = await _reportCardService.GetStudentReportCardsAsync(studentId);
                var reportCardDtos = reportCards.Select(MapToDto);
                return Ok(reportCardDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving report cards for student {StudentId}", studentId);
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Delete ALL report cards (Admin only, irreversible!)
        /// </summary>
        [HttpDelete("all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAllReportCards()
        {
            try
            {
                await _reportCardService.DeleteAllReportCardsAsync();
                _logger.LogWarning("All report cards deleted by admin {AdminId}", GetCurrentUserId());
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all report cards");
                return HandleException(ex);
            }
        }

        
        [HttpGet("download/class/{gradeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadClassReportCardsZip(
            int gradeId, 
            [FromQuery] int academicYear, 
            [FromQuery] int term)
        {
            var adminId = GetCurrentUserId();

            try
            {
                var resolvedYear = await ResolveAcademicYear(academicYear);
                if (resolvedYear == null)
                {
                    return BadRequest(new { message = $"Invalid academic year: {academicYear}" });
                }

                var reportCards = await _reportCardService.GetExistingReportCardsAsync(gradeId, resolvedYear.Value, term);

                if (!reportCards.Any())
                {
                    return NotFound(new { message = "No report cards found for this class." });
                }

                var zipBytes = await CreateZipFile(reportCards);
                var fileName = $"ReportCards_Grade{gradeId}_{resolvedYear.Value}_Term{term}.zip";

                return File(zipBytes, "application/zip", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating ZIP file for grade {GradeId}", gradeId);
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Request merged PDF generation (returns jobId)
        /// </summary>
        [HttpPost("download/class/{gradeId}/merged/request")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RequestMergedPdf(
            int gradeId, 
            [FromQuery] int academicYear, 
            [FromQuery] int term)
        {
            try
            {
                var adminId = GetCurrentUserId();
                var resolvedYear = await ResolveAcademicYear(academicYear);
                if (resolvedYear == null)
                {
                    return BadRequest(new { message = $"Invalid academic year: {academicYear}" });
                }
                var jobId = _backgroundJobClient.Enqueue(() => GenerateAndStoreMergedPdf(gradeId, resolvedYear.Value, term, adminId));
                return Ok(new { jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting merged PDF for grade {GradeId}", gradeId);
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Check job status for merged PDF generation
        /// </summary>
        [HttpGet("download/class/merged/status/{jobId}")]
        [Authorize(Roles = "Admin")]
        public ActionResult CheckMergedPdfStatus(string jobId)
        {
            try
            {
                var connection = JobStorage.Current.GetConnection();
                var jobDetails = connection.GetJobData(jobId);

                if (jobDetails == null)
                {
                    return NotFound(new { status = "not_found", message = "Job not found" });
                }

                var status = MapJobStatus(jobDetails.State);
                return Ok(new { jobId, status, message = jobDetails.State });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking status for job {JobId}", jobId);
                return StatusCode(500, new { status = "error", message = "Error checking job status" });
            }
        }

       
        [HttpGet("download/class/merged/file/{gradeId}/{academicYear}/{term}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DownloadMergedPdfFile(int gradeId, int academicYear, int term)
        {
            try
            {
                // Use the same logic as in RequestMergedPdf to resolve the year
                var resolvedYear = await ResolveAcademicYear(academicYear);
                if (resolvedYear == null)
                {
                    return BadRequest(new { message = $"Invalid academic year: {academicYear}" });
                }
        
                var fileName = $"ReportCards_Grade{gradeId}_{resolvedYear.Value}_Term{term}_Merged.pdf";
                var filePath = GetMergedPdfPath(fileName);
        
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Merged PDF file not found at path: {FilePath}", filePath);
                    return NotFound(new { message = "Merged PDF not ready yet." });
                }
        
                var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.DeleteOnClose);
                return File(stream, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading merged PDF file for Grade {GradeId}", gradeId);
                return HandleException(ex);
            }
        }

     
        [HttpGet("{reportCardId}/view")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult> ViewReportCard(int reportCardId)
        {
            try
            {
                var pdfBytes = await _reportCardService.GetReportCardPdfAsync(reportCardId);
                if (pdfBytes == null || pdfBytes.Length == 0)
                {
                    return NotFound(new { message = "PDF not found for this report card." });
                }

                Response.Headers["Content-Disposition"] = $"inline; filename=ReportCard_{reportCardId}.pdf";
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing report card {ReportCardId}", reportCardId);
                return HandleException(ex);
            }
        }

       
        [HttpGet("class/{gradeId}/view-urls")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<object>>> GetClassReportCardViewUrls(
            int gradeId, 
            [FromQuery] int academicYear, 
            [FromQuery] int term)
        {
            try
            {
                var adminId = GetCurrentUserId();
                var resolvedYear = await ResolveAcademicYear(academicYear);
                
                if (resolvedYear == null)
                {
                    return BadRequest(new { message = $"Invalid academic year: {academicYear}" });
                }

                //var reportCards = await _reportCardService.GenerateClassReportCardsAsync(gradeId, resolvedYear.Value, term, adminId);
                var reportCards = await _reportCardService.GetExistingReportCardsAsync(gradeId, resolvedYear.Value, term);

                var urls = reportCards.Select(rc => new {
                    rc.Id,
                    rc.AcademicYear,
                    rc.Term,
                    GradeName = rc.Grade?.FullName ?? rc.Grade?.Name ?? "",
                    StudentName = rc.Student?.FullName ?? "",
                    ViewUrl = Url.Action("ViewReportCard", "ReportCards", new { reportCardId = rc.Id }, Request.Scheme)
                });

                return Ok(urls);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting view URLs for grade {GradeId}", gradeId);
                return HandleException(ex);
            }
        }

        [HttpGet("class/{gradeId}/report-status")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<object>> GetClassReportStatus(
            int gradeId, 
            [FromQuery] int academicYear, 
            [FromQuery] int term)
        {
            try
            {
                var resolvedYear = await ResolveAcademicYear(academicYear);
                if (resolvedYear == null)
                {
                    return BadRequest(new { message = $"Invalid academic year: {academicYear}" });
                }

                // Fetch each dependency sequentially to avoid DbContext concurrency issues
                var students = await _reportCardService.GetStudentsInGradeAsync(gradeId);
                var existingReports = await _reportCardService.GetExistingReportCardsAsync(gradeId, resolvedYear.Value, term);
                var grade = await _reportCardService.GetGradeAsync(gradeId);

                // Create lookup for existing reports
                var existingReportLookup = existingReports.ToDictionary(r => r.StudentId, r => r);

                var result = students.Select(s => new {
                    studentId = s.Id,
                    studentName = s.FullName ?? "",
                    gradeName = grade?.FullName ?? grade?.Name ?? "",
                    reportCardId = existingReportLookup.TryGetValue(s.Id, out var report) ? report.Id : (int?)null,
                    hasReportCard = existingReportLookup.ContainsKey(s.Id),
                    generatedAt = existingReportLookup.TryGetValue(s.Id, out var r) ? r.GeneratedAt : (DateTime?)null
                });

                return Ok(new {
                    gradeId,
                    gradeName = grade?.FullName ?? grade?.Name ?? "",
                    academicYear = resolvedYear.Value,
                    term,
                    totalStudents = students.Count(),
                    generatedReports = existingReports.Count(),
                    students = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report status for grade {GradeId}", gradeId);
                return HandleException(ex);
            }
        }

        /// <summary>
        /// Enqueue background job to send merged report cards to configured email address (Admin only)
        /// </summary>
        [HttpPost("send/class/{gradeId}/email")]
        [Authorize(Roles = "Admin")]
        public IActionResult SendClassReportCardsEmail(
            int gradeId, 
            [FromQuery] int academicYear, 
            [FromQuery] int term)
        {
            var adminId = GetCurrentUserId();
            var jobId = _backgroundJobClient.Enqueue(() => SendClassReportCardsEmailBackground(gradeId, academicYear, term, adminId));
            return Ok(new { jobId, message = "Report card email is being processed in the background." });
        }

        // Background job method for sending merged PDF
        [NonAction]
        public async Task SendClassReportCardsEmailBackground(int gradeId, int academicYear, int term, int adminId)
        {
            string toEmail = _configuration["ReportCardEmail"];
            string gradeName = $"Grade {gradeId}";
            int? resolvedYear = null;
            int studentCount = 0;
            string mergedPdfFileName = string.Empty;
            string adminFullName = "System";
            try
            {
                // Fetch admin full name
                var adminUser = await _userService.GetUserByIdAsync(adminId);
                if (adminUser != null && !string.IsNullOrWhiteSpace(adminUser.FullName))
                    adminFullName = adminUser.FullName;

                resolvedYear = await ResolveAcademicYear(academicYear);
                if (resolvedYear == null)
                {
                    _logger.LogError("Invalid academic year: {AcademicYear}", academicYear);
                    await SendNotificationEmail(toEmail, false, gradeName, academicYear, term, 0, null, "Invalid academic year");
                    return;
                }

                // Get existing report cards
                var reportCards = await _reportCardService.GetExistingReportCardsAsync(gradeId, resolvedYear.Value, term);
                studentCount = reportCards.Count();
                if (!reportCards.Any())
                {
                    _logger.LogWarning("No report cards found for grade {GradeId}, year {Year}, term {Term}", gradeId, resolvedYear.Value, term);
                    await SendNotificationEmail(toEmail, false, gradeName, resolvedYear.Value, term, 0, null, "No report cards found for this class.");
                    return;
                }

                // Get grade information for email subject
                var grade = await _reportCardService.GetGradeAsync(gradeId);
                gradeName = grade?.FullName ?? grade?.Name ?? $"Grade {gradeId}";

                // Create merged PDF
                var mergedPdfBytes = await CreateMergedPdfBytes(reportCards);
                mergedPdfFileName = $"ReportCards_{gradeName}_{resolvedYear.Value}_Term{term}_Merged.pdf";

                // Send email with only the merged PDF attachment
                var subject = $"Report Cards - {gradeName} - {resolvedYear.Value} Term {term}";
                var body = $@"
Dear Administrator,

Please find attached the merged report cards for {gradeName} for {resolvedYear.Value} Term {term}.

This email contains:
- A merged PDF file containing all report cards in a single document

Total students: {reportCards.Count()}

Generated by: {adminFullName}
Generated at: {DateTime.Now:dd-MM-yyyy HH:mm:ss}

Best regards,
BluebirdCore System
";

                await _emailService.SendEmailWithAttachmentAsync(
                    toEmail,
                    subject,
                    body,
                    mergedPdfBytes,
                    mergedPdfFileName
                );

                _logger.LogInformation("Report cards email sent to {Email} for grade {GradeId}", toEmail, gradeId);
                await SendNotificationEmail(toEmail, true, gradeName, resolvedYear.Value, term, studentCount, mergedPdfFileName, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending report cards email for grade {GradeId}", gradeId);
                await SendNotificationEmail(toEmail, false, gradeName, resolvedYear ?? academicYear, term, studentCount, mergedPdfFileName, ex.Message);
            }
        }

        // Helper method to send notification email to admin
        [NonAction]
        private async Task SendNotificationEmail(string toEmail, bool success, string gradeName, int academicYear, int term, int studentCount, string? mergedPdfFileName, string? errorMessage)
        {
            var subject = success
                ? $"[BluebirdCore] Report Card Email Sent Successfully - {gradeName} {academicYear} Term {term}"
                : $"[BluebirdCore] Report Card Email FAILED - {gradeName} {academicYear} Term {term}";
            var body = success
                ? $@"The merged report card email for {gradeName} ({academicYear} Term {term}) was sent successfully.\n\nTotal students: {studentCount}\nAttachment: {mergedPdfFileName}\nTime: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\nBest regards,\nBluebirdCore System"
                : $@"The merged report card email for {gradeName} ({academicYear} Term {term}) FAILED.\n\nError: {errorMessage}\nTime: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\nBest regards,\nBluebirdCore System";
            await _emailService.SendEmailWithAttachmentAsync(
                toEmail,
                subject,
                body,
                Array.Empty<byte>(), // No attachment for notification
                ""
            );
        }

        #endregion

        #region Helper Methods

        // Background job method
        [NonAction]
        public async Task GenerateAndStoreMergedPdf(int gradeId, int academicYear, int term, int adminId)
        {
            try
            {
                var fileName = $"ReportCards_Grade{gradeId}_{academicYear}_Term{term}_Merged.pdf";
                var filePath = GetMergedPdfPath(fileName);

                var reportCards = await _reportCardService.GenerateClassReportCardsAsync(gradeId, academicYear, term, adminId);
                if (!reportCards.Any())
                {
                    _logger.LogWarning("No report cards found for grade {GradeId}", gradeId);
                    return;
                }

                using (var outputDoc = new PdfDocument())
                {
                    foreach (var rc in reportCards)
                    {
                        var pdfBytes = await _reportCardService.GetReportCardPdfAsync(rc.Id);
                        if (pdfBytes?.Length > 0)
                        {
                            using (var inputStream = new MemoryStream(pdfBytes))
                            {
                                var inputDoc = PdfReader.Open(inputStream, PdfDocumentOpenMode.Import);
                                for (int i = 0; i < inputDoc.PageCount; i++)
                                {
                                    outputDoc.AddPage(inputDoc.Pages[i]);
                                }
                            }
                        }
                    }
                    outputDoc.Save(filePath);
                }

                _logger.LogInformation("Merged PDF generated successfully: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating merged PDF for grade {GradeId}", gradeId);
                throw;
            }
        }

#if DEBUG
        /// <summary>
        /// Test SMTP email sending
        /// </summary>
        [HttpPost("test-smtp")]
        [AllowAnonymous]
        public async Task<IActionResult> TestSmtp([FromQuery] string toEmail)
        {
            try
            {
                await _reportCardService.TestSmtpSend(toEmail);
                return Ok(new { message = $"Test email sent to {toEmail}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP test failed");
                return StatusCode(500, new { message = "SMTP test failed", error = ex.Message });
            }
        }
#endif

        // Helper methods
        private int GetCurrentUserId()
        {
            return int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private static ReportCardDto MapToDto(dynamic reportCard)
        {
            return new ReportCardDto
            {
                Id = reportCard.Id,
                StudentId = reportCard.StudentId,
                StudentName = reportCard.Student?.FullName ?? "",
                GradeName = reportCard.Grade?.FullName ?? "",
                AcademicYear = reportCard.AcademicYear,
                Term = reportCard.Term,
                GeneratedAt = reportCard.GeneratedAt,
                GeneratedByName = reportCard.GeneratedByUser?.FullName ?? ""
            };
        }

        private ActionResult HandleException(Exception ex)
        {
            return ex switch
            {
                ArgumentException => BadRequest(new { message = ex.Message }),
                UnauthorizedAccessException => Unauthorized(new { message = "Access denied" }),
                InvalidOperationException ioEx => StatusCode(500, new { message = ioEx.Message, error = "Operation failed" }),
                _ => StatusCode(500, new { message = ex.Message ?? "An error occurred while processing your request", error = "Internal server error" })
            };
        }

        private async Task<int?> ResolveAcademicYear(int academicYear)
        {
            if (academicYear >= MinimumValidYear)
            {
                return academicYear;
            }

            // Treat as ID and look up the year
            var yearEntity = await _reportCardService.GetAcademicYearByIdAsync(academicYear);
            if (yearEntity != null && int.TryParse(yearEntity.Name, out var year) && year >= MinimumValidYear)
            {
                return year;
            }

            return null;
        }

        private string GetMergedPdfPath(string fileName)
        {
            var tempDir = Path.Combine(Path.GetTempPath(), MergedPdfTempDirectory);
            Directory.CreateDirectory(tempDir);
            return Path.Combine(tempDir, fileName);
        }

        private static string MapJobStatus(string jobState)
        {
            return jobState?.ToLower() switch
            {
                "processing" => "processing",
                "succeeded" => "completed",
                "failed" => "failed",
                "scheduled" => "scheduled",
                _ => "unknown"
            };
        }

        private async Task<byte[]> CreateZipFile(IEnumerable<dynamic> reportCards)
        {
            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
            {
                foreach (var rc in reportCards)
                {
                    var pdfBytes = await _reportCardService.GetReportCardPdfAsync(rc.Id);
                    if (pdfBytes?.Length > 0)
                    {
                        var entry = zip.CreateEntry($"ReportCard_{rc.Student?.FullName ?? rc.StudentId.ToString()}.pdf");
                        using var entryStream = entry.Open();
                        await entryStream.WriteAsync(pdfBytes, 0, pdfBytes.Length);
                    }
                }
            }
            return ms.ToArray();
        }

        private async Task<byte[]> CreateMergedPdfBytes(IEnumerable<dynamic> reportCards)
        {
            using var ms = new MemoryStream();
            using (var outputDoc = new PdfDocument())
            {
                foreach (var rc in reportCards)
                {
                    var pdfBytes = await _reportCardService.GetReportCardPdfAsync(rc.Id);
                    if (pdfBytes?.Length > 0)
                    {
                        using (var inputStream = new MemoryStream(pdfBytes))
                        {
                            var inputDoc = PdfReader.Open(inputStream, PdfDocumentOpenMode.Import);
                            for (int i = 0; i < inputDoc.PageCount; i++)
                            {
                                outputDoc.AddPage(inputDoc.Pages[i]);
                            }
                        }
                    }
                }
                outputDoc.Save(ms);
            }
            return ms.ToArray();
        }

        #endregion

        #region General Comment Management

        /// <summary>
        /// Get the general comment for a report card
        /// </summary>
        /// <param name="reportCardId">The report card ID</param>
        /// <returns>The general comment if it exists</returns>
        [HttpGet("{reportCardId}/general-comment")]
        public async Task<ActionResult<string>> GetGeneralComment(int reportCardId)
        {
            try
            {
                var comment = await _reportCardService.GetGeneralCommentAsync(reportCardId);
                return Ok(comment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving general comment for report card {ReportCardId}", reportCardId);
                return StatusCode(500, "Error retrieving general comment");
            }
        }

        /// <summary>
        /// Update the general comment for a report card
        /// </summary>
        /// <param name="reportCardId">The report card ID</param>
        /// <param name="request">The comment update request</param>
        /// <returns>Success status</returns>
        [HttpPut("{reportCardId}/general-comment")]
        public async Task<ActionResult> UpdateGeneralComment(int reportCardId, [FromBody] UpdateGeneralCommentRequest request)
        {
            try
            {
                // Get current user ID from JWT token
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                // Validate request
                if (string.IsNullOrWhiteSpace(request.Comment))
                {
                    return BadRequest("Comment cannot be empty");
                }

                if (request.Comment.Length > 2000)
                {
                    return BadRequest("Comment cannot exceed 2000 characters");
                }

                // Check if teacher can edit this comment
                var canEdit = await _reportCardService.CanTeacherEditGeneralCommentAsync(reportCardId, currentUserId);
                if (!canEdit)
                {
                    return Forbid("You are not authorized to edit general comments for this report card");
                }

                // Update the comment
                await _reportCardService.UpdateGeneralCommentAsync(reportCardId, request.Comment, currentUserId);

                _logger.LogInformation("General comment updated for report card {ReportCardId} by user {UserId}", 
                    reportCardId, currentUserId);

                return Ok(new { message = "General comment updated successfully" });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid request for updating general comment for report card {ReportCardId}", reportCardId);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized attempt to update general comment for report card {ReportCardId}", reportCardId);
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating general comment for report card {ReportCardId}", reportCardId);
                return StatusCode(500, "Error updating general comment");
            }
        }

        /// <summary>
        /// Check if the current user can edit the general comment for a report card
        /// </summary>
        /// <param name="reportCardId">The report card ID</param>
        /// <returns>Whether the user can edit the comment</returns>
        [HttpGet("{reportCardId}/general-comment/can-edit")]
        public async Task<ActionResult<bool>> CanEditGeneralComment(int reportCardId)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                if (currentUserId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                var canEdit = await _reportCardService.CanTeacherEditGeneralCommentAsync(reportCardId, currentUserId);
                return Ok(canEdit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking edit permissions for general comment for report card {ReportCardId}", reportCardId);
                return StatusCode(500, "Error checking edit permissions");
            }
        }


        #endregion
    }

    public class UpdateGeneralCommentRequest
    {
        public string Comment { get; set; } = string.Empty;
    }
}