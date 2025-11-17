using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using BluebirdCore.Data;
using Microsoft.EntityFrameworkCore;
using BluebirdCore.Services;

namespace BluebirdCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MarkScheduleController : ControllerBase
    {
        private readonly MarkSchedulePdfService _markSchedulePdfService;
        private readonly ILogger<MarkScheduleController> _logger;
        private readonly SchoolDbContext _context;

        public MarkScheduleController(MarkSchedulePdfService markSchedulePdfService, ILogger<MarkScheduleController> logger, SchoolDbContext context)
        {
            _markSchedulePdfService = markSchedulePdfService;
            _logger = logger;
            _context = context;
        }

        [HttpGet("pdf")]
        public async Task<IActionResult> GetMarkSchedulePdf(
            [FromQuery] int academicYearId,
            [FromQuery] int term,
            [FromQuery] string examTypeName)
        {
            try
            {
                var pdfBytes = await _markSchedulePdfService.GenerateMarkSchedulePdfAsync(academicYearId, term, examTypeName);
                var fileName = $"MarkSchedule_AllGrades_Year{academicYearId}_Term{term}_{examTypeName}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "No data found for mark schedule PDF generation");
                return BadRequest(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error generating mark schedule PDF");
                return StatusCode(500, "An error occurred while generating the mark schedule PDF.");
            }
        }

        [HttpGet("pdf/grade/{gradeId}")]
        public async Task<IActionResult> GetMarkSchedulePdfForGrade(
            [FromRoute] int gradeId,
            [FromQuery] int academicYearId,
            [FromQuery] int term,
            [FromQuery] string examTypeName)
        {
            try
            {
                // Verify the grade exists
                var grade = await _context.Grades
                    .FirstOrDefaultAsync(g => g.Id == gradeId && g.IsActive);
                
                if (grade == null)
                    return NotFound($"Grade with ID {gradeId} not found or is not active");

                var pdfBytes = await _markSchedulePdfService.GenerateMarkSchedulePdfForGradeAsync(gradeId, academicYearId, term, examTypeName);
                var fileName = $"MarkSchedule_{grade.Name}_{grade.Stream}_Year{academicYearId}_Term{term}_{examTypeName}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "No data found for mark schedule PDF generation for grade {GradeId}", gradeId);
                return BadRequest(new { error = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error generating mark schedule PDF for grade {GradeId}", gradeId);
                return StatusCode(500, "An error occurred while generating the mark schedule PDF.");
            }
        }
    }
} 