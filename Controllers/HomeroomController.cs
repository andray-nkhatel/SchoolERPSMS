using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolErpSMS.DTOs;
using SchoolErpSMS.Services;
using SchoolErpSMS.Models;
using System.Security.Claims;

namespace SchoolErpSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Teacher")]
    public class HomeroomController : ControllerBase
    {
        private readonly IHomeroomService _homeroomService;

        public HomeroomController(IHomeroomService homeroomService)
        {
            _homeroomService = homeroomService;
        }

        /// <summary>
        /// Get current teacher's homeroom students with their subjects
        /// </summary>
        [HttpGet("students")]
        public async Task<ActionResult<ApiResponse<List<HomeroomStudentDto>>>> GetHomeroomStudents()
        {
            try
            {
                var teacherId = GetCurrentTeacherId();
                if (teacherId == null)
                    return Unauthorized(new ApiResponse<List<HomeroomStudentDto>>
                    {
                        Success = false,
                        Message = "Unable to identify teacher"
                    });

                var students = await _homeroomService.GetHomeroomStudentsAsync(teacherId.Value);
                
                if (!students.Any())
                {
                    return Ok(new ApiResponse<List<HomeroomStudentDto>>
                    {
                        Success = true,
                        Data = students,
                        Message = "You are not assigned as a homeroom teacher or have no students"
                    });
                }

                return Ok(new ApiResponse<List<HomeroomStudentDto>>
                {
                    Success = true,
                    Data = students,
                    Message = $"Retrieved {students.Count} homeroom students"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<HomeroomStudentDto>>
                {
                    Success = false,
                    Message = $"Error retrieving homeroom students: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get homeroom grade information and available subjects
        /// </summary>
        [HttpGet("grade-info")]
        public async Task<ActionResult<ApiResponse<HomeroomGradeInfoDto>>> GetHomeroomGradeInfo()
        {
            try
            {
                var teacherId = GetCurrentTeacherId();
                if (teacherId == null)
                    return Unauthorized(new ApiResponse<HomeroomGradeInfoDto>
                    {
                        Success = false,
                        Message = "Unable to identify teacher"
                    });

                var gradeInfo = await _homeroomService.GetHomeroomGradeInfoAsync(teacherId.Value);
                
                return Ok(new ApiResponse<HomeroomGradeInfoDto>
                {
                    Success = true,
                    Data = gradeInfo,
                    Message = "Homeroom grade information retrieved successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<HomeroomGradeInfoDto>
                {
                    Success = false,
                    Message = $"Error retrieving grade information: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Assign subject to a student in homeroom
        /// </summary>
        [HttpPost("students/{studentId}/subjects")]
        public async Task<ActionResult<ApiResponse<StudentSubjectDto>>> AssignSubjectToStudent(
            int studentId, [FromBody] AssignSubjectDto dto)
        {
            try
            {
                var teacherId = GetCurrentTeacherId();
                if (teacherId == null)
                    return Unauthorized(new ApiResponse<StudentSubjectDto>
                    {
                        Success = false,
                        Message = "Unable to identify teacher"
                    });

                var result = await _homeroomService.AssignSubjectToStudentAsync(teacherId.Value, studentId, dto);
                
                if (!result.Success)
                {
                    return result.Message.Contains("homeroom") ? Forbid(result.Message) : BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StudentSubjectDto>
                {
                    Success = false,
                    Message = $"Error assigning subject: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Remove subject from a student in homeroom
        /// </summary>
        [HttpDelete("students/{studentId}/subjects/{subjectId}")]
        public async Task<ActionResult<ApiResponse<object>>> RemoveSubjectFromStudent(
            int studentId, int subjectId, [FromBody] RemoveSubjectDto dto)
        {
            try
            {
                var teacherId = GetCurrentTeacherId();
                if (teacherId == null)
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unable to identify teacher"
                    });

                var result = await _homeroomService.RemoveSubjectFromStudentAsync(teacherId.Value, studentId, subjectId, dto);
                
                if (!result.Success)
                {
                    return result.Message.Contains("homeroom") ? Forbid(result.Message) : BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error removing subject: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Bulk assign subject to multiple students in homeroom
        /// </summary>
        [HttpPost("bulk-assign-subjects")]
        public async Task<ActionResult<ApiResponse<object>>> BulkAssignSubjects([FromBody] BulkAssignSubjectsDto dto)
        {
            try
            {
                var teacherId = GetCurrentTeacherId();
                if (teacherId == null)
                    return Unauthorized(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Unable to identify teacher"
                    });

                var result = await _homeroomService.BulkAssignSubjectsAsync(teacherId.Value, dto);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error in bulk assignment: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get available subjects for homeroom grade
        /// </summary>
        [HttpGet("available-subjects")]
        public async Task<ActionResult<ApiResponse<List<AvailableSubjectDto>>>> GetAvailableSubjects()
        {
            try
            {
                var teacherId = GetCurrentTeacherId();
                if (teacherId == null)
                    return Unauthorized(new ApiResponse<List<AvailableSubjectDto>>
                    {
                        Success = false,
                        Message = "Unable to identify teacher"
                    });

                var gradeInfo = await _homeroomService.GetHomeroomGradeInfoAsync(teacherId.Value);
                
                return Ok(new ApiResponse<List<AvailableSubjectDto>>
                {
                    Success = true,
                    Data = gradeInfo.AvailableSubjects,
                    Message = "Available subjects retrieved successfully"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<AvailableSubjectDto>>
                {
                    Success = false,
                    Message = $"Error retrieving available subjects: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Debug endpoint to check homeroom teacher status
        /// </summary>
        [HttpGet("debug/status")]
        public async Task<ActionResult<object>> DebugHomeroomStatus()
        {
            try
            {
                var teacherId = GetCurrentTeacherId();
                if (teacherId == null)
                    return Unauthorized(new { message = "Unable to identify teacher" });

                var homeroomGrades = await _homeroomService.GetTeacherHomeroomGradesAsync(teacherId.Value);
                var students = await _homeroomService.GetHomeroomStudentsAsync(teacherId.Value);

                return Ok(new
                {
                    TeacherId = teacherId,
                    HomeroomGrades = homeroomGrades,
                    StudentCount = students.Count,
                    IsHomeroomTeacher = homeroomGrades.Any(),
                    Claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error checking status: {ex.Message}" });
            }
        }

        /// <summary>
        /// Update a student's name (homeroom teachers only for their grade)
        /// </summary>
        [HttpPatch("students/{studentId}/name")]
        public async Task<ActionResult<ApiResponse<HomeroomStudentDto>>> UpdateStudentName(int studentId, [FromBody] UpdateStudentNameDto dto)
        {
            try
            {
                var teacherId = GetCurrentTeacherId();
                if (teacherId == null)
                    return Unauthorized(new ApiResponse<HomeroomStudentDto>
                    {
                        Success = false,
                        Message = "Unable to identify teacher"
                    });

                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<HomeroomStudentDto>
                    {
                        Success = false,
                        Message = "Invalid name data"
                    });
                }

                var result = await _homeroomService.UpdateStudentNameAsync(teacherId.Value, studentId, dto);
                if (!result.Success)
                {
                    return result.Message.Contains("homeroom") ? Forbid(result.Message) : BadRequest(result);
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<HomeroomStudentDto>
                {
                    Success = false,
                    Message = $"Error updating student name: {ex.Message}"
                });
            }
        }

        // Helper method to get current teacher ID from JWT claims
        private int? GetCurrentTeacherId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return null;
            return userId;
        }
    }
}
