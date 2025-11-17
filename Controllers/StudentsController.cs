using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BluebirdCore.Data;
using BluebirdCore.Entities;
using BluebirdCore.DTOs;
using BluebirdCore.Models;
using BluebirdCore.Services;

namespace BluebirdCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly SchoolDbContext _context;

        public StudentsController(IStudentService studentService, SchoolDbContext context)
        {
            _studentService = studentService;
            _context = context;
        }

        /// <summary>
        /// Get all students
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StudentDto>>>> GetStudents()
        {
            try
            {
                var students = await _context.Students
                    .Include(s => s.Grade)
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .Select(s => new StudentDto
                    {
                        Id = s.Id,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        MiddleName = s.MiddleName,
                        StudentNumber = s.StudentNumber,
                        DateOfBirth = s.DateOfBirth ?? DateTime.MinValue,
                        Gender = s.Gender,
                        Address = s.Address,
                        PhoneNumber = s.PhoneNumber,
                        GuardianName = s.GuardianName,
                        GuardianPhone = s.GuardianPhone,
                        GradeId = s.GradeId,
                        GradeName = s.Grade != null ? s.Grade.FullName : null,
                        IsActive = s.IsActive,
                        IsArchived = s.IsArchived,
                        EnrollmentDate = s.EnrollmentDate,
                        FullName = s.FullName,
                        OptionalSubjects = s.OptionalSubjects != null ? s.OptionalSubjects.Select(os => new SubjectDto
                        {
                            Id = os.Subject.Id,
                            Name = os.Subject.Name,
                            Code = os.Subject.Code
                        }).ToList() : new List<SubjectDto>()
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<StudentDto>>
                {
                    Success = true,
                    Data = students,
                    Message = "Students retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<StudentDto>>
                {
                    Success = false,
                    Message = $"Error retrieving students: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get a specific student by ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<ApiResponse<StudentDto>>> GetStudent(int id)
        {
            try
            {
                var student = await _studentService.GetStudentByIdAsync(id);
                if (student == null)
                {
                    return NotFound(new ApiResponse<StudentDto>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                var studentDto = new StudentDto
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    DateOfBirth = student.DateOfBirth ?? DateTime.MinValue,
                    Gender = student.Gender,
                    Address = student.Address,
                    PhoneNumber = student.PhoneNumber,
                    GuardianName = student.GuardianName,
                    GuardianPhone = student.GuardianPhone,
                    GradeId = student.GradeId,
                    GradeName = student.Grade != null ? student.Grade.FullName : null,
                    IsActive = student.IsActive,
                    IsArchived = student.IsArchived,
                    EnrollmentDate = student.EnrollmentDate,
                    FullName = student.FullName,
                    OptionalSubjects = student.OptionalSubjects != null ? student.OptionalSubjects.Select(os => new SubjectDto
                    {
                        Id = os.Subject.Id,
                        Name = os.Subject.Name,
                        Code = os.Subject.Code
                    }).ToList() : new List<SubjectDto>()
                };

                return Ok(new ApiResponse<StudentDto>
                {
                    Success = true,
                    Data = studentDto,
                    Message = "Student retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StudentDto>
                {
                    Success = false,
                    Message = $"Error retrieving student: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Create a new student
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ApiResponse<StudentDto>>> CreateStudent([FromBody] CreateStudentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<StudentDto>
                    {
                        Success = false,
                        Message = "Invalid student data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var student = new Student
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    Address = dto.Address,
                    PhoneNumber = dto.PhoneNumber,
                    GuardianName = dto.GuardianName,
                    GuardianPhone = dto.GuardianPhone,
                    GradeId = dto.GradeId,
                    IsActive = true,
                    IsArchived = false,
                    EnrollmentDate = DateTime.Now
                };

                var createdStudent = await _studentService.CreateStudentAsync(student);
                
                var studentDto = new StudentDto
                {
                    Id = createdStudent.Id,
                    FirstName = createdStudent.FirstName,
                    LastName = createdStudent.LastName,
                    DateOfBirth = createdStudent.DateOfBirth ?? DateTime.MinValue,
                    Gender = createdStudent.Gender,
                    Address = createdStudent.Address,
                    PhoneNumber = createdStudent.PhoneNumber,
                    GuardianName = createdStudent.GuardianName,
                    GuardianPhone = createdStudent.GuardianPhone,
                    GradeId = createdStudent.GradeId,
                    GradeName = createdStudent.Grade != null ? createdStudent.Grade.FullName : null,
                    IsActive = createdStudent.IsActive,
                    IsArchived = createdStudent.IsArchived,
                    EnrollmentDate = createdStudent.EnrollmentDate,
                    FullName = createdStudent.FullName,
                    OptionalSubjects = new List<SubjectDto>()
                };

                return CreatedAtAction(nameof(GetStudent), new { id = createdStudent.Id }, new ApiResponse<StudentDto>
                {
                    Success = true,
                    Data = studentDto,
                    Message = "Student created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StudentDto>
                {
                    Success = false,
                    Message = $"Error creating student: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Update a student
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ApiResponse<StudentDto>>> UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<StudentDto>
                    {
                        Success = false,
                        Message = "Invalid student data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var existingStudent = await _studentService.GetStudentByIdAsync(id);
                if (existingStudent == null)
                {
                    return NotFound(new ApiResponse<StudentDto>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                // Update the existing student with new data
                existingStudent.FirstName = dto.FirstName;
                existingStudent.LastName = dto.LastName;
                existingStudent.DateOfBirth = dto.DateOfBirth;
                existingStudent.Gender = dto.Gender;
                existingStudent.Address = dto.Address;
                existingStudent.PhoneNumber = dto.PhoneNumber;
                existingStudent.GuardianName = dto.GuardianName;
                existingStudent.GuardianPhone = dto.GuardianPhone;
                existingStudent.GradeId = dto.GradeId;

                var student = await _studentService.UpdateStudentAsync(existingStudent);
                
                var studentDto = new StudentDto
                {
                    Id = student.Id,
                    FirstName = student.FirstName,
                    LastName = student.LastName,
                    DateOfBirth = student.DateOfBirth ?? DateTime.MinValue,
                    Gender = student.Gender,
                    Address = student.Address,
                    PhoneNumber = student.PhoneNumber,
                    GuardianName = student.GuardianName,
                    GuardianPhone = student.GuardianPhone,
                    GradeId = student.GradeId,
                    GradeName = student.Grade != null ? student.Grade.FullName : null,
                    IsActive = student.IsActive,
                    IsArchived = student.IsArchived,
                    EnrollmentDate = student.EnrollmentDate,
                    FullName = student.FullName,
                    OptionalSubjects = student.OptionalSubjects != null ? student.OptionalSubjects.Select(os => new SubjectDto
                    {
                        Id = os.Subject.Id,
                        Name = os.Subject.Name,
                        Code = os.Subject.Code
                    }).ToList() : new List<SubjectDto>()
                };

                return Ok(new ApiResponse<StudentDto>
                {
                    Success = true,
                    Data = studentDto,
                    Message = "Student updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StudentDto>
                {
                    Success = false,
                    Message = $"Error updating student: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Delete a student (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteStudent(int id)
        {
            try
            {
                var success = await _studentService.DeleteStudentAsync(id);
                if (!success)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Student deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error deleting student: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get students by grade
        /// </summary>
        [HttpGet("grade/{gradeId}")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StudentDto>>>> GetStudentsByGrade(int gradeId)
        {
            try
            {
                var students = await _context.Students
                    .Include(s => s.Grade)
                    .Where(s => s.GradeId == gradeId && s.IsActive)
                    .OrderBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .Select(s => new StudentDto
                    {
                        Id = s.Id,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        MiddleName = s.MiddleName,
                        StudentNumber = s.StudentNumber,
                        GradeId = s.GradeId,
                        GradeName = s.Grade != null ? s.Grade.FullName : null,
                        FullName = s.FullName
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<IEnumerable<StudentDto>>
                {
                    Success = true,
                    Data = students,
                    Message = "Students retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<StudentDto>>
                {
                    Success = false,
                    Message = $"Error retrieving students: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Assign optional subjects to a student
        /// </summary>
        [HttpPost("{id}/assign-optional-subjects")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ApiResponse<object>>> AssignOptionalSubjects(int id, [FromBody] AssignOptionalSubjectsDto assignDto)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.Grade)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (student == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                // Get maximum optional subjects for the grade section
                var maxOptionalSubjects = student.Grade?.Section switch
                {
                    SchoolSection.PrimaryLower => 0,
                    SchoolSection.PrimaryUpper => 2,
                    SchoolSection.SecondaryJunior => 3,
                    SchoolSection.SecondarySenior => 3,
                    _ => 0
                };

                if (assignDto.SubjectIds.Count > maxOptionalSubjects)
                {
                    return BadRequest(new { message = $"Maximum {maxOptionalSubjects} optional subjects allowed for {student.Grade.Section}" });
                }

                // Validate that all subjects are optional
                var validOptionalSubjects = await _context.GradeSubjects
                    .Where(gs => gs.GradeId == student.GradeId && gs.IsOptional && gs.IsActive)
                    .Select(gs => gs.SubjectId)
                    .ToListAsync();

                var invalidSubjects = assignDto.SubjectIds.Except(validOptionalSubjects).ToList();
                if (invalidSubjects.Any())
                {
                    return BadRequest(new { message = "Some subjects are not optional for this grade" });
                }

                // Remove existing optional subjects
                var existingOptionalSubjects = await _context.StudentOptionalSubjects
                    .Where(sos => sos.StudentId == id)
                    .ToListAsync();

                _context.StudentOptionalSubjects.RemoveRange(existingOptionalSubjects);

                // Add new optional subjects
                var newOptionalSubjects = assignDto.SubjectIds.Select(subjectId => new StudentOptionalSubject
                {
                    StudentId = id,
                    SubjectId = subjectId
                }).ToList();

                _context.StudentOptionalSubjects.AddRange(newOptionalSubjects);
                await _context.SaveChangesAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Optional subjects assigned successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error assigning optional subjects: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get students with their grades information - for report cards
        /// </summary>
        [HttpGet("grades")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<ApiResponse<object>>> GetStudentsWithGrades()
        {
            try
            {
                var students = await _context.Students
                    .Include(s => s.Grade)
                    .Where(s => s.IsActive && !s.IsArchived)
                    .OrderBy(s => s.Grade.Level)
                    .ThenBy(s => s.Grade.Stream)
                    .ThenBy(s => s.LastName)
                    .ThenBy(s => s.FirstName)
                    .Select(s => new
                    {
                        Id = s.Id,
                        FirstName = s.FirstName,
                        LastName = s.LastName,
                        MiddleName = s.MiddleName,
                        StudentNumber = s.StudentNumber,
                        FullName = s.FullName,
                        GradeId = s.GradeId,
                        GradeName = s.Grade != null ? s.Grade.FullName : null,
                        GradeLevel = s.Grade != null ? s.Grade.Level : 0,
                        GradeStream = s.Grade != null ? s.Grade.Stream : null,
                        GradeSection = s.Grade != null ? s.Grade.Section.ToString() : null,
                        IsActive = s.IsActive,
                        IsArchived = s.IsArchived
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = students,
                    Message = "Students with grades retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error retrieving students with grades: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Bulk assign optional subjects to multiple students
        /// </summary>
        [HttpPost("bulk-assign-optional-subjects")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ApiResponse<List<BulkAssignmentResult>>>> BulkAssignOptionalSubjects([FromBody] BulkAssignOptionalSubjectsDto bulkDto)
        {
            try
            {
                var results = new List<BulkAssignmentResult>();
                var errorCount = 0;

                foreach (var studentId in bulkDto.StudentIds)
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

                        // Get maximum optional subjects for the grade section
                        var maxOptionalSubjects = student.Grade?.Section switch
                        {
                            SchoolSection.PrimaryLower => 0,
                            SchoolSection.PrimaryUpper => 2,
                            SchoolSection.SecondaryJunior => 3,
                            SchoolSection.SecondarySenior => 3,
                            _ => 0
                        };

                        if (bulkDto.SubjectIds.Count > maxOptionalSubjects)
                        {
                            results.Add(new BulkAssignmentResult
                            {
                                StudentId = studentId,
                                Success = false,
                                Message = $"Maximum {maxOptionalSubjects} optional subjects allowed for {student.Grade.Section}"
                            });
                            errorCount++;
                            continue;
                        }

                        // Validate that all subjects are optional
                        var validOptionalSubjects = await _context.GradeSubjects
                            .Where(gs => gs.GradeId == student.GradeId && gs.IsOptional && gs.IsActive)
                            .Select(gs => gs.SubjectId)
                            .ToListAsync();

                        var invalidSubjects = bulkDto.SubjectIds.Except(validOptionalSubjects).ToList();
                        if (invalidSubjects.Any())
                        {
                            results.Add(new BulkAssignmentResult
                            {
                                StudentId = studentId,
                                Success = false,
                                Message = "Some subjects are not optional for this grade"
                            });
                            errorCount++;
                            continue;
                        }

                        // Remove existing optional subjects
                        var existingOptionalSubjects = await _context.StudentOptionalSubjects
                            .Where(sos => sos.StudentId == studentId)
                            .ToListAsync();

                        _context.StudentOptionalSubjects.RemoveRange(existingOptionalSubjects);

                        // Add new optional subjects
                        var newOptionalSubjects = bulkDto.SubjectIds.Select(subjectId => new StudentOptionalSubject
                        {
                            StudentId = studentId,
                            SubjectId = subjectId
                        }).ToList();

                        _context.StudentOptionalSubjects.AddRange(newOptionalSubjects);
                        await _context.SaveChangesAsync();

                        results.Add(new BulkAssignmentResult
                        {
                            StudentId = studentId,
                            Success = true,
                            Message = "Optional subjects assigned successfully"
                        });
                    }
                    catch (Exception ex)
                    {
                        results.Add(new BulkAssignmentResult
                        {
                            StudentId = studentId,
                            Success = false,
                            Message = ex.Message
                        });
                        errorCount++;
                    }
                }

                return Ok(new ApiResponse<List<BulkAssignmentResult>>
                {
                    Success = true,
                    Data = results,
                    Message = $"Bulk assignment completed. {results.Count - errorCount} successful, {errorCount} errors."
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a minimal student (firstName, lastName, gradeId only)
        /// </summary>
        [HttpPost("minimal")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ApiResponse<StudentDto>>> CreateMinimalStudent([FromBody] CreateMinimalStudentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<StudentDto>
                    {
                        Success = false,
                        Message = "Invalid student data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                // Verify grade exists
                var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Id == dto.GradeId);
                if (grade == null)
                {
                    return BadRequest(new ApiResponse<StudentDto>
                    {
                        Success = false,
                        Message = $"Grade with ID {dto.GradeId} not found"
                    });
                }

                var student = new Student
                {
                    FirstName = dto.FirstName.Trim(),
                    LastName = dto.LastName.Trim(),
                    GradeId = dto.GradeId,
                    IsActive = true,
                    IsArchived = false,
                    EnrollmentDate = DateTime.Now
                };

                var createdStudent = await _studentService.CreateStudentAsync(student);
                
                var studentDto = new StudentDto
                {
                    Id = createdStudent.Id,
                    FirstName = createdStudent.FirstName,
                    LastName = createdStudent.LastName,
                    GradeId = createdStudent.GradeId,
                    GradeName = createdStudent.Grade != null ? createdStudent.Grade.FullName : null,
                    IsActive = createdStudent.IsActive,
                    IsArchived = createdStudent.IsArchived,
                    EnrollmentDate = createdStudent.EnrollmentDate,
                    FullName = createdStudent.FullName,
                    OptionalSubjects = new List<SubjectDto>()
                };

                return CreatedAtAction(nameof(GetStudent), new { id = createdStudent.Id }, new ApiResponse<StudentDto>
                {
                    Success = true,
                    Data = studentDto,
                    Message = "Student created successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StudentDto>
                {
                    Success = false,
                    Message = $"Error creating student: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Update a student minimally (firstName, lastName, gradeId, isActive)
        /// </summary>
        [HttpPut("minimal/{id}")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<ApiResponse<StudentDto>>> UpdateMinimalStudent(int id, [FromBody] UpdateMinimalStudentDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiResponse<StudentDto>
                    {
                        Success = false,
                        Message = "Invalid student data",
                        Errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()
                    });
                }

                var existingStudent = await _studentService.GetStudentByIdAsync(id);
                if (existingStudent == null)
                {
                    return NotFound(new ApiResponse<StudentDto>
                    {
                        Success = false,
                        Message = "Student not found"
                    });
                }

                // Verify grade exists if gradeId is being changed
                if (dto.GradeId != existingStudent.GradeId)
                {
                    var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Id == dto.GradeId);
                    if (grade == null)
                    {
                        return BadRequest(new ApiResponse<StudentDto>
                        {
                            Success = false,
                            Message = $"Grade with ID {dto.GradeId} not found"
                        });
                    }
                }

                // Update only the minimal fields
                existingStudent.FirstName = dto.FirstName.Trim();
                existingStudent.LastName = dto.LastName.Trim();
                existingStudent.GradeId = dto.GradeId;
                existingStudent.IsActive = dto.IsActive;

                var updatedStudent = await _studentService.UpdateStudentAsync(existingStudent);
                
                var studentDto = new StudentDto
                {
                    Id = updatedStudent.Id,
                    FirstName = updatedStudent.FirstName,
                    LastName = updatedStudent.LastName,
                    GradeId = updatedStudent.GradeId,
                    GradeName = updatedStudent.Grade != null ? updatedStudent.Grade.FullName : null,
                    IsActive = updatedStudent.IsActive,
                    IsArchived = updatedStudent.IsArchived,
                    EnrollmentDate = updatedStudent.EnrollmentDate,
                    FullName = updatedStudent.FullName,
                    OptionalSubjects = updatedStudent.OptionalSubjects != null ? updatedStudent.OptionalSubjects.Select(os => new SubjectDto
                    {
                        Id = os.Subject.Id,
                        Name = os.Subject.Name,
                        Code = os.Subject.Code
                    }).ToList() : new List<SubjectDto>()
                };

                return Ok(new ApiResponse<StudentDto>
                {
                    Success = true,
                    Data = studentDto,
                    Message = "Student updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<StudentDto>
                {
                    Success = false,
                    Message = $"Error updating student: {ex.Message}"
                });
            }
        }
    }
}