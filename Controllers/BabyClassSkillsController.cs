using BluebirdCore.DTOs;
using BluebirdCore.Entities;
using BluebirdCore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BluebirdCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BabyClassSkillsController : ControllerBase
    {
        private readonly IBabyClassSkillService _skillService;
        private readonly ILogger<BabyClassSkillsController> _logger;

        public BabyClassSkillsController(IBabyClassSkillService skillService, ILogger<BabyClassSkillsController> logger)
        {
            _skillService = skillService;
            _logger = logger;
        }

        /// <summary>
        /// Get all baby class skills
        /// </summary>
        [HttpGet("skills")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<BabyClassSkillDto>>> GetSkills()
        {
            try
            {
                var skills = await _skillService.GetAllSkillsAsync();
                var skillDtos = skills.Select(s => new BabyClassSkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Order = s.Order,
                    IsActive = s.IsActive
                });

                return Ok(skillDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving baby class skills");
                return StatusCode(500, "An error occurred while retrieving skills");
            }
        }

        /// <summary>
        /// Get skill items for a specific skill
        /// </summary>
        [HttpGet("skills/{skillId}/items")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<BabyClassSkillItemDto>>> GetSkillItems(int skillId)
        {
            try
            {
                var skillItems = await _skillService.GetSkillItemsBySkillIdAsync(skillId);
                var skillItemDtos = skillItems.Select(si => new BabyClassSkillItemDto
                {
                    Id = si.Id,
                    Name = si.Name,
                    Description = si.Description,
                    SkillId = si.SkillId,
                    Order = si.Order,
                    IsActive = si.IsActive
                });

                return Ok(skillItemDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving skill items for skill {SkillId}", skillId);
                return StatusCode(500, "An error occurred while retrieving skill items");
            }
        }

        /// <summary>
        /// Get student skill assessments
        /// </summary>
        [HttpGet("students/{studentId}/assessments")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<BabyClassSkillAssessmentDto>>> GetStudentAssessments(
            int studentId, 
            [FromQuery] int academicYear, 
            [FromQuery] int term)
        {
            try
            {
                _logger.LogInformation("Getting assessments for StudentId: {StudentId}, AcademicYear: {AcademicYear}, Term: {Term}", 
                    studentId, academicYear, term);

                var assessments = await _skillService.GetStudentAssessmentsAsync(studentId, academicYear, term);
                
                _logger.LogInformation("Retrieved {Count} assessments from database", assessments.Count());

                var assessmentDtos = assessments.Select(a => new BabyClassSkillAssessmentDto
                {
                    Id = a.Id,
                    StudentId = a.StudentId,
                    SkillItemId = a.SkillItemId,
                    AcademicYear = a.AcademicYear,
                    Term = a.Term,
                    TeacherComment = a.TeacherComment,
                    AssessedAt = a.AssessedAt,
                    AssessedBy = a.AssessedBy,
                    StudentName = $"{a.Student.FirstName} {a.Student.LastName}",
                    SkillItemName = a.SkillItem.Name,
                    SkillName = a.SkillItem.Skill.Name,
                    AssessedByTeacherName = null
                });

                _logger.LogInformation("Successfully mapped {Count} assessment DTOs", assessmentDtos.Count());
                return Ok(assessmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving assessments for student {StudentId}, AcademicYear: {AcademicYear}, Term: {Term}. " +
                    "Exception Type: {ExceptionType}, Message: {Message}, StackTrace: {StackTrace}", 
                    studentId, academicYear, term, ex.GetType().Name, ex.Message, ex.StackTrace);
                return StatusCode(500, $"An error occurred while retrieving assessments: {ex.Message}");
            }
        }

        /// <summary>
        /// Create or update a skill assessment
        /// </summary>
        [HttpPost("assessments")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<BabyClassSkillAssessmentDto>> CreateOrUpdateAssessment([FromBody] CreateSkillAssessmentDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                var assessment = await _skillService.CreateOrUpdateAssessmentAsync(
                    dto.StudentId, 
                    dto.SkillItemId, 
                    dto.AcademicYear, 
                    dto.Term, 
                    dto.TeacherComment, 
                    userId);

                var assessmentDto = new BabyClassSkillAssessmentDto
                {
                    Id = assessment.Id,
                    StudentId = assessment.StudentId,
                    SkillItemId = assessment.SkillItemId,
                    AcademicYear = assessment.AcademicYear,
                    Term = assessment.Term,
                    TeacherComment = assessment.TeacherComment,
                    AssessedAt = assessment.AssessedAt,
                    AssessedBy = assessment.AssessedBy
                };

                return Ok(assessmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating/updating assessment for student {StudentId}", dto.StudentId);
                return StatusCode(500, "An error occurred while saving the assessment");
            }
        }

        [HttpPut("assessments")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<BabyClassSkillAssessmentDto>> UpdateAssessment([FromBody] UpdateSkillAssessmentDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                // Get the existing assessment by ID
                var existingAssessment = await _skillService.GetAssessmentByIdAsync(dto.AssessmentId);
                
                if (existingAssessment == null)
                    return NotFound("Assessment not found");

                var assessment = await _skillService.CreateOrUpdateAssessmentAsync(
                    existingAssessment.StudentId, 
                    existingAssessment.SkillItemId, 
                    existingAssessment.AcademicYear, 
                    existingAssessment.Term, 
                    dto.TeacherComment, 
                    userId);

                var assessmentDto = new BabyClassSkillAssessmentDto
                {
                    Id = assessment.Id,
                    StudentId = assessment.StudentId,
                    SkillItemId = assessment.SkillItemId,
                    AcademicYear = assessment.AcademicYear,
                    Term = assessment.Term,
                    TeacherComment = assessment.TeacherComment,
                    AssessedAt = assessment.AssessedAt,
                    AssessedBy = assessment.AssessedBy,
                    StudentName = $"{assessment.Student.FirstName} {assessment.Student.LastName}",
                    SkillItemName = assessment.SkillItem.Name,
                    SkillName = assessment.SkillItem.Skill.Name,
                    AssessedByTeacherName = null
                };

                return Ok(assessmentDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assessment {AssessmentId}", dto.AssessmentId);
                return StatusCode(500, "An error occurred while updating the assessment");
            }
        }


        /// <summary>
        /// Delete a skill assessment
        /// </summary>
        [HttpDelete("assessments/{assessmentId}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult> DeleteAssessment(int assessmentId)
        {
            try
            {
                var result = await _skillService.DeleteAssessmentAsync(assessmentId);
                if (!result)
                    return NotFound("Assessment not found");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assessment {AssessmentId}", assessmentId);
                return StatusCode(500, "An error occurred while deleting the assessment");
            }
        }

        /// <summary>
        /// Get class skill assessments
        /// </summary>
        [HttpGet("classes/{gradeId}/assessments")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<StudentSkillAssessmentSummaryDto>>> GetClassAssessments(
            int gradeId, 
            [FromQuery] int academicYear, 
            [FromQuery] int term)
        {
            try
            {
                var assessments = await _skillService.GetClassAssessmentsAsync(gradeId, academicYear, term);
                
                var groupedAssessments = assessments
                    .GroupBy(a => new { a.StudentId, a.Student.FirstName, a.Student.LastName, a.Student.GradeId, a.Student.Grade.Name })
                    .Select(g => new StudentSkillAssessmentSummaryDto
                    {
                        StudentId = g.Key.StudentId,
                        StudentName = $"{g.Key.FirstName} {g.Key.LastName}",
                        GradeId = g.Key.GradeId,
                        GradeName = g.Key.Name,
                        Assessments = g.Select(a => new BabyClassSkillAssessmentDto
                        {
                            Id = a.Id,
                            StudentId = a.StudentId,
                            SkillItemId = a.SkillItemId,
                            AcademicYear = a.AcademicYear,
                            Term = a.Term,
                            TeacherComment = a.TeacherComment,
                            AssessedAt = a.AssessedAt,
                            AssessedBy = a.AssessedBy,
                            StudentName = $"{a.Student.FirstName} {a.Student.LastName}",
                            SkillItemName = a.SkillItem.Name,
                            SkillName = a.SkillItem.Skill.Name,
                            AssessedByTeacherName = null
                        }).ToList()
                    });

                return Ok(groupedAssessments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving class assessments for grade {GradeId}", gradeId);
                return StatusCode(500, "An error occurred while retrieving class assessments");
            }
        }
    }
}
