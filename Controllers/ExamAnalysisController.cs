using Microsoft.AspNetCore.Mvc;
using BluebirdCore.Services;
using Microsoft.AspNetCore.Authorization;

namespace BluebirdCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExamAnalysisController : ControllerBase
    {
        private readonly ExamAnalysisPdfService _examAnalysisPdfService;
        private readonly ILogger<ExamAnalysisController> _logger;

        public ExamAnalysisController(ExamAnalysisPdfService examAnalysisPdfService, ILogger<ExamAnalysisController> logger)
        {
            _examAnalysisPdfService = examAnalysisPdfService;
            _logger = logger;
        }

        /// <summary>
        /// Generate Exam Analysis PDF for all secondary grades
        /// </summary>
        /// <param name="academicYearId">Academic Year ID</param>
        /// <param name="term">Term (1, 2, or 3)</param>
        /// <param name="examTypeName">Exam Type Name (e.g., "End-of-Term")</param>
        /// <returns>PDF file</returns>
        [HttpGet("pdf")]
        public async Task<IActionResult> GetExamAnalysisPdf(
            [FromQuery] int academicYearId,
            [FromQuery] int term,
            [FromQuery] string examTypeName = "End-of-Term")
        {
            try
            {
                var pdfBytes = await _examAnalysisPdfService.GenerateExamAnalysisPdfAsync(academicYearId, term, examTypeName);
                
                return File(pdfBytes, "application/pdf", $"ExamAnalysis_{academicYearId}_Term{term}_{examTypeName}.pdf");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "No data found for exam analysis PDF generation");
                return BadRequest(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error generating exam analysis PDF");
                return StatusCode(500, "An error occurred while generating the exam analysis PDF.");
            }
        }

        /// <summary>
        /// Generate Exam Analysis PDF for a specific secondary grade
        /// </summary>
        /// <param name="gradeId">Grade ID</param>
        /// <param name="academicYearId">Academic Year ID</param>
        /// <param name="term">Term (1, 2, or 3)</param>
        /// <param name="examTypeName">Exam Type Name (e.g., "End-of-Term")</param>
        /// <returns>PDF file</returns>
        [HttpGet("pdf/{gradeId}")]
        public async Task<IActionResult> GetExamAnalysisPdfForGrade(
            int gradeId,
            [FromQuery] int academicYearId,
            [FromQuery] int term,
            [FromQuery] string examTypeName = "End-of-Term")
        {
            try
            {
                var pdfBytes = await _examAnalysisPdfService.GenerateExamAnalysisPdfForGradeAsync(gradeId, academicYearId, term, examTypeName);
                
                return File(pdfBytes, "application/pdf", $"ExamAnalysis_Grade{gradeId}_{academicYearId}_Term{term}_{examTypeName}.pdf");
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "No data found for exam analysis PDF generation for grade {GradeId}", gradeId);
                return BadRequest(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error generating exam analysis PDF for grade {GradeId}", gradeId);
                return StatusCode(500, "An error occurred while generating the exam analysis PDF.");
            }
        }
    }
} 