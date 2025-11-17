using BluebirdCore.DTOs;
using BluebirdCore.Entities;
using BluebirdCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BluebirdCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AcademicYearsController : ControllerBase
    {
        private readonly IAcademicYearService _academicYearService;
        private readonly ILogger<AcademicYearsController> _logger;

        public AcademicYearsController(IAcademicYearService academicYearService, ILogger<AcademicYearsController> logger)
        {
            _academicYearService = academicYearService;
            _logger = logger;
        }

       
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<AcademicYear>>> GetAcademicYears()
        {
            var academicYears = await _academicYearService.GetAllAcademicYearsAsync();
            return Ok(academicYears);
        }

      
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<AcademicYear>> GetAcademicYear(int id)
        {
            var academicYear = await _academicYearService.GetAcademicYearByIdAsync(id);
            if (academicYear == null)
                return NotFound();

            return Ok(academicYear);
        }

      
        [HttpGet("active")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<AcademicYear>> GetActiveAcademicYear()
        {
            var activeYear = await _academicYearService.GetActiveAcademicYearAsync();
            if (activeYear == null)
                return NotFound();

            return Ok(activeYear);
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AcademicYear>> CreateAcademicYear([FromBody] CreateAcademicYearDto createAcademicYearDto)
        {
            try
            {
                if (createAcademicYearDto == null)
                    return BadRequest("Academic year data is required");

                var academicYear = new AcademicYear
                {
                    Name = createAcademicYearDto.Name,
                    StartDate = createAcademicYearDto.StartDate,
                    EndDate = createAcademicYearDto.EndDate
                };

                var createdYear = await _academicYearService.CreateAcademicYearAsync(academicYear);
                return CreatedAtAction(nameof(GetAcademicYear), new { id = createdYear.Id }, createdYear);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating academic year");
                return StatusCode(500, "An error occurred while creating the academic year");
            }
        }

       
       
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteAcademicYear(int id)
        {
            try
            {
                var existingAcademicYear = await _academicYearService.GetAcademicYearByIdAsync(id);
                if (existingAcademicYear == null)
                    return NotFound();

                var success = await _academicYearService.DeleteAcademicYearAsync(id);
                if (!success)
                    return BadRequest("Failed to delete academic year");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting academic year with ID {Id}", id);
                return StatusCode(500, "An error occurred while deleting the academic year");
            }
        }

      
        [HttpPost("{academicYearId}/close")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> CloseAcademicYear(int academicYearId)
        {
            var success = await _academicYearService.CloseAcademicYearAsync(academicYearId);
            if (!success)
                return NotFound();

            return Ok(new { message = "Academic year closed successfully" });
        }

    
    }


}