using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SchoolErpSMS.Data;
using SchoolErpSMS.Entities;
using SchoolErpSMS.DTOs;
using SchoolErpSMS.Models;

namespace SchoolErpSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Teacher")]
    public class SecondarySubjectAssignmentController : ControllerBase
    {
        private readonly SchoolDbContext _context;

        public SecondarySubjectAssignmentController(SchoolDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all secondary students with their current subject assignments
        /// </summary>
        [HttpGet("students")]
        public async Task<ActionResult<ApiResponse<List<SecondaryStudentWithSubjectsDto>>>> GetSecondaryStudentsWithSubjects(
            string? search = null)
        {
            try
            {
                var query = _context.Students
                    .Include(s => s.Grade)
                    .Include(s => s.StudentSubjects)
                        .ThenInclude(ss => ss.Subject)
                    .Where(s => s.Grade.Section == SchoolSection.SecondaryJunior || 
                               s.Grade.Section == SchoolSection.SecondarySenior);

                if (!string.IsNullOrEmpty(search))
                {
                    var searchLower = search.ToLower();
                    query = query.Where(s => 
                        s.FirstName.ToLower().Contains(searchLower) ||
                        s.LastName.ToLower().Contains(searchLower) ||
                        s.StudentNumber.ToLower().Contains(searchLower));
                }

                var students = await query
                    .OrderBy(s => s.Grade.Section)
                    .ThenBy(s => s.Grade.Level)
                    .ThenBy(s => s.FirstName)
                    .ToListAsync();

                var result = students.Select(s => new SecondaryStudentWithSubjectsDto
                {
                    Id = s.Id,
                    FullName = $"{s.FirstName} {s.LastName}",
                    StudentNumber = s.StudentNumber,
                    GradeName = s.Grade?.FullName ?? "Unknown",
                    Section = s.Grade?.Section.ToString() ?? "Unknown",
                    SubjectCount = s.StudentSubjects.Count(ss => ss.IsActive),
                    Subjects = s.StudentSubjects
                        .Where(ss => ss.IsActive)
                        .Select(ss => new StudentSubjectDto
                        {
                            Id = ss.Id,
                            StudentId = ss.StudentId,
                            SubjectId = ss.SubjectId,
                            SubjectName = ss.Subject?.Name ?? "Unknown",
                            SubjectCode = ss.Subject?.Code ?? "",
                            EnrolledDate = ss.EnrolledDate,
                            CompletedDate = ss.CompletedDate,
                            IsActive = ss.IsActive,
                            Notes = ss.Notes,
                            AssignedBy = ss.AssignedBy
                        })
                        .ToList(),
                    LastUpdated = s.StudentSubjects.Any() ? s.StudentSubjects.Max(ss => ss.AssignedDate) : s.EnrollmentDate
                }).ToList();

                return Ok(new ApiResponse<List<SecondaryStudentWithSubjectsDto>>
                {
                    Success = true,
                    Data = result,
                    Message = $"Retrieved {result.Count} secondary students"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<SecondaryStudentWithSubjectsDto>>
                {
                    Success = false,
                    Message = $"Error retrieving secondary students: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get individual student's subject assignments
        /// </summary>
        [HttpGet("students/{studentId}/subjects")]
        public async Task<ActionResult<ApiResponse<List<StudentSubjectDto>>>> GetStudentSubjects(int studentId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.Grade)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                {
                    return NotFound(new ApiResponse<List<StudentSubjectDto>>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                // Check if student is in secondary section
                if (student.Grade?.Section != SchoolSection.SecondaryJunior && 
                    student.Grade?.Section != SchoolSection.SecondarySenior)
                {
                    return BadRequest(new ApiResponse<List<StudentSubjectDto>>
                    {
                        Success = false,
                        Message = "This endpoint is only for secondary students"
                    });
                }

                var studentSubjects = await _context.StudentSubjects
                    .Include(ss => ss.Subject)
                    .Where(ss => ss.StudentId == studentId)
                    .OrderBy(ss => ss.Subject.Name)
                    .ToListAsync();

                var result = studentSubjects.Select(ss => new StudentSubjectDto
                {
                    Id = ss.Id,
                    StudentId = ss.StudentId,
                    SubjectId = ss.SubjectId,
                    SubjectName = ss.Subject?.Name ?? "Unknown",
                    SubjectCode = ss.Subject?.Code ?? "",
                    EnrolledDate = ss.EnrolledDate,
                    CompletedDate = ss.CompletedDate,
                    IsActive = ss.IsActive,
                    Notes = ss.Notes,
                    AssignedBy = ss.AssignedBy
                }).ToList();

                return Ok(new ApiResponse<List<StudentSubjectDto>>
                {
                    Success = true,
                    Data = result,
                    Message = $"Retrieved {result.Count} subjects for student"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<StudentSubjectDto>>
                {
                    Success = false,
                    Message = $"Error retrieving student subjects: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get all available subjects
        /// </summary>
        [HttpGet("subjects")]
        public async Task<ActionResult<ApiResponse<List<SubjectDto>>>> GetAllSubjects()
        {
            try
            {
                var subjects = await _context.Subjects
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.Name)
                    .Select(s => new SubjectDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Code = s.Code,
                        Description = s.Description,
                        IsActive = s.IsActive
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<List<SubjectDto>>
                {
                    Success = true,
                    Data = subjects,
                    Message = $"Retrieved {subjects.Count} subjects"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<SubjectDto>>
                {
                    Success = false,
                    Message = $"Error retrieving subjects: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Assign subject to secondary student
        /// </summary>
        [HttpPost("students/{studentId}/subjects")]
        public async Task<ActionResult<ApiResponse<StudentSubjectDto>>> AssignSubjectToStudent(
            int studentId, AssignSecondarySubjectDto dto)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.Grade)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                {
                    return NotFound(new ApiResponse<StudentSubjectDto>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                // Check if student is in secondary section
                if (student.Grade?.Section != SchoolSection.SecondaryJunior && 
                    student.Grade?.Section != SchoolSection.SecondarySenior)
                {
                    return BadRequest(new ApiResponse<StudentSubjectDto>
                    {
                        Success = false,
                        Message = "This endpoint is only for secondary students"
                    });
                }

                var subject = await _context.Subjects
                    .FirstOrDefaultAsync(s => s.Id == dto.SubjectId);

                if (subject == null)
                {
                    return NotFound(new ApiResponse<StudentSubjectDto>
                    {
                        Success = false,
                        Message = "Subject not found"
                    });
                }

                // Check if subject is already assigned to this student
                var existingAssignment = await _context.StudentSubjects
                    .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SubjectId == dto.SubjectId);

                StudentSubject studentSubject;
                
                if (existingAssignment != null)
                {
                    // If there's an inactive assignment, reactivate it
                    if (!existingAssignment.IsActive)
                    {
                        existingAssignment.IsActive = true;
                        existingAssignment.DroppedDate = null;
                        existingAssignment.AssignedBy = dto.AssignedBy;
                        existingAssignment.AssignedDate = DateTime.UtcNow;
                        existingAssignment.Notes = dto.Notes;
                        studentSubject = existingAssignment;
                    }
                    else
                    {
                        return BadRequest(new ApiResponse<StudentSubjectDto>
                        {
                            Success = false,
                            Message = "Subject is already assigned to this student"
                        });
                    }
                }
                else
                {
                    studentSubject = new StudentSubject
                    {
                        StudentId = studentId,
                        SubjectId = dto.SubjectId,
                        Notes = dto.Notes,
                        AssignedBy = dto.AssignedBy,
                        AssignedDate = DateTime.UtcNow,
                        EnrolledDate = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.StudentSubjects.Add(studentSubject);
                }
                
                await _context.SaveChangesAsync();

                var result = new StudentSubjectDto
                {
                    Id = studentSubject.Id,
                    StudentId = studentSubject.StudentId,
                    SubjectId = studentSubject.SubjectId,
                    SubjectName = subject.Name,
                    SubjectCode = subject.Code,
                    EnrolledDate = studentSubject.EnrolledDate,
                    CompletedDate = studentSubject.CompletedDate,
                    IsActive = studentSubject.IsActive,
                    Notes = studentSubject.Notes,
                    AssignedBy = studentSubject.AssignedBy
                };

                return Ok(new ApiResponse<StudentSubjectDto>
                {
                    Success = true,
                    Data = result,
                    Message = $"Subject '{subject.Name}' assigned successfully"
                });
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
        /// Remove subject from secondary student
        /// </summary>
        [HttpDelete("students/{studentId}/subjects/{subjectId}")]
        public async Task<ActionResult<ApiResponse<object>>> RemoveSubjectFromStudent(
            int studentId, int subjectId)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.Grade)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                // Check if student is in secondary section
                if (student.Grade?.Section != SchoolSection.SecondaryJunior && 
                    student.Grade?.Section != SchoolSection.SecondarySenior)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "This endpoint is only for secondary students"
                    });
                }

                var studentSubject = await _context.StudentSubjects
                    .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SubjectId == subjectId);

                if (studentSubject == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Subject assignment not found"
                    });
                }

                _context.StudentSubjects.Remove(studentSubject);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Subject removed successfully"
                });
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
        /// Bulk assign subjects to multiple secondary students
        /// </summary>
        [HttpPost("bulk-assign")]
        public async Task<ActionResult<ApiResponse<List<BulkAssignmentResult>>>> BulkAssignSubjects(
            BulkAssignSecondarySubjectsDto dto)
        {
            try
            {
                var results = new List<BulkAssignmentResult>();
                var errorCount = 0;

                foreach (var studentId in dto.StudentIds)
                {
                    try
                    {
                        var student = await _context.Students
                            .Include(s => s.Grade)
                            .FirstOrDefaultAsync(s => s.Id == studentId);

                        if (student == null)
                        {
                            results.Add(new BulkAssignmentResult
                            {
                                StudentId = studentId,
                                Success = false,
                                Message = "Student not found"
                            });
                            errorCount++;
                            continue;
                        }

                        // Check if student is in secondary section
                        if (student.Grade?.Section != SchoolSection.SecondaryJunior && 
                            student.Grade?.Section != SchoolSection.SecondarySenior)
                        {
                            results.Add(new BulkAssignmentResult
                            {
                                StudentId = studentId,
                                Success = false,
                                Message = "Student is not in secondary section"
                            });
                            errorCount++;
                            continue;
                        }

                        var assignedSubjects = new List<string>();

                        foreach (var subjectId in dto.SubjectIds)
                        {
                            // Check if subject is already assigned
                            var existingAssignment = await _context.StudentSubjects
                                .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SubjectId == subjectId);

                            if (existingAssignment == null)
                            {
                                var studentSubject = new StudentSubject
                                {
                                    StudentId = studentId,
                                    SubjectId = subjectId,
                                    Notes = dto.Notes,
                                    AssignedBy = dto.AssignedBy,
                                    AssignedDate = DateTime.UtcNow,
                                    EnrolledDate = DateTime.UtcNow,
                                    IsActive = true
                                };

                                _context.StudentSubjects.Add(studentSubject);
                                assignedSubjects.Add($"Subject {subjectId}");
                            }
                        }

                        await _context.SaveChangesAsync();

                        results.Add(new BulkAssignmentResult
                        {
                            StudentId = studentId,
                            Success = true,
                            Message = $"Assigned {assignedSubjects.Count} subjects"
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new BulkAssignmentResult
                        {
                            StudentId = studentId,
                            Success = false,
                            Message = $"Error: {ex.Message}"
                        });
                        errorCount++;
                    }
                }

                return Ok(new ApiResponse<List<BulkAssignmentResult>>
                {
                    Success = true,
                    Data = results,
                    Message = $"Bulk assignment completed. {results.Count - errorCount} successful, {errorCount} errors"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<List<BulkAssignmentResult>>
                {
                    Success = false,
                    Message = $"Error in bulk assignment: {ex.Message}"
                });
            }
        }
    }
}
