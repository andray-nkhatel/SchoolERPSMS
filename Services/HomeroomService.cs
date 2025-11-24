using Microsoft.EntityFrameworkCore;
using SchoolErpSMS.Data;
using SchoolErpSMS.DTOs;
using SchoolErpSMS.Entities;
using SchoolErpSMS.Models;
using System.Security.Claims;

namespace SchoolErpSMS.Services
{
    public interface IHomeroomService
    {
        Task<List<int>> GetTeacherHomeroomGradesAsync(int teacherId);
        Task<bool> CanAccessStudentAsync(int teacherId, int studentId);
        Task<List<HomeroomStudentDto>> GetHomeroomStudentsAsync(int teacherId);
        Task<HomeroomGradeInfoDto> GetHomeroomGradeInfoAsync(int teacherId);
        Task<ApiResponse<StudentSubjectDto>> AssignSubjectToStudentAsync(int teacherId, int studentId, AssignSubjectDto dto);
        Task<ApiResponse<object>> RemoveSubjectFromStudentAsync(int teacherId, int studentId, int subjectId, RemoveSubjectDto dto);
        Task<ApiResponse<object>> BulkAssignSubjectsAsync(int teacherId, BulkAssignSubjectsDto dto);
        Task<ApiResponse<HomeroomStudentDto>> UpdateStudentNameAsync(int teacherId, int studentId, UpdateStudentNameDto dto);
    }

    public class HomeroomService : IHomeroomService
    {
        private readonly SchoolDbContext _context;

        public HomeroomService(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<List<int>> GetTeacherHomeroomGradesAsync(int teacherId)
        {
            return await _context.Grades
                .Where(g => g.HomeroomTeacherId == teacherId && g.IsActive)
                .Select(g => g.Id)
                .ToListAsync();
        }

        public async Task<bool> CanAccessStudentAsync(int teacherId, int studentId)
        {
            var homeroomGradeIds = await GetTeacherHomeroomGradesAsync(teacherId);
            
            return await _context.Students
                .Where(s => s.Id == studentId && homeroomGradeIds.Contains(s.GradeId))
                .AnyAsync();
        }

        public async Task<List<HomeroomStudentDto>> GetHomeroomStudentsAsync(int teacherId)
        {
            var homeroomGradeIds = await GetTeacherHomeroomGradesAsync(teacherId);
            
            if (!homeroomGradeIds.Any())
                return new List<HomeroomStudentDto>();

            var students = await _context.Students
                .Include(s => s.Grade)
                .Include(s => s.StudentSubjects)
                    .ThenInclude(ss => ss.Subject)
                .Where(s => homeroomGradeIds.Contains(s.GradeId) && s.IsActive)
                .OrderBy(s => s.FirstName)
                .ThenBy(s => s.LastName)
                .ToListAsync();

            return students.Select(s => new HomeroomStudentDto
            {
                Id = s.Id,
                StudentNumber = s.StudentNumber,
                FirstName = s.FirstName,
                LastName = s.LastName,
                FullName = $"{s.FirstName} {s.LastName}",
                GradeName = s.Grade.FullName,
                GradeId = s.GradeId,
                Subjects = s.StudentSubjects
                    .Where(ss => ss.IsActive)
                    .Select(ss => new StudentSubjectDto
                    {
                        Id = ss.Id,
                        StudentId = ss.StudentId,
                        SubjectId = ss.SubjectId,
                        SubjectName = ss.Subject.Name,
                        SubjectCode = ss.Subject.Code,
                        EnrolledDate = ss.EnrolledDate,
                        CompletedDate = ss.CompletedDate,
                        IsActive = ss.IsActive,
                        Notes = ss.Notes,
                        AssignedBy = ss.AssignedBy
                    }).ToList()
            }).ToList();
        }

        public async Task<HomeroomGradeInfoDto> GetHomeroomGradeInfoAsync(int teacherId)
        {
            var homeroomGrades = await _context.Grades
                .Include(g => g.Students)
                .Include(g => g.GradeSubjects)
                    .ThenInclude(gs => gs.Subject)
                .Where(g => g.HomeroomTeacherId == teacherId && g.IsActive)
                .ToListAsync();

            if (!homeroomGrades.Any())
                throw new UnauthorizedAccessException("You are not assigned as a homeroom teacher");

            // For simplicity, we'll use the first homeroom grade
            // In a real scenario, you might want to handle multiple grades
            var grade = homeroomGrades.First();
            var studentCount = grade.Students.Count(s => s.IsActive);

            var availableSubjects = await _context.GradeSubjects
                .Include(gs => gs.Subject)
                .Where(gs => gs.GradeId == grade.Id && gs.IsActive)
                .Select(gs => new AvailableSubjectDto
                {
                    Id = gs.Subject.Id,
                    Name = gs.Subject.Name,
                    Code = gs.Subject.Code,
                    Description = gs.Subject.Description,
                    IsOptional = gs.IsOptional,
                    IsAssigned = false // This would need to be calculated based on current assignments
                })
                .ToListAsync();

            return new HomeroomGradeInfoDto
            {
                GradeId = grade.Id,
                GradeName = grade.FullName,
                Section = grade.Section.ToString(),
                Level = grade.Level,
                StudentCount = studentCount,
                AvailableSubjects = availableSubjects
            };
        }

        public async Task<ApiResponse<StudentSubjectDto>> AssignSubjectToStudentAsync(int teacherId, int studentId, AssignSubjectDto dto)
        {
            try
            {
                // Security check: Can this teacher access this student?
                if (!await CanAccessStudentAsync(teacherId, studentId))
                {
                    return new ApiResponse<StudentSubjectDto>
                    {
                        Success = false,
                        Message = "You can only manage students in your homeroom"
                    };
                }

                // Check if subject is already assigned
                var existingAssignment = await _context.StudentSubjects
                    .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SubjectId == dto.SubjectId);

                if (existingAssignment != null)
                {
                    return new ApiResponse<StudentSubjectDto>
                    {
                        Success = false,
                        Message = "Subject is already assigned to this student"
                    };
                }

                // Get teacher name for audit trail
                var teacher = await _context.Users.FindAsync(teacherId);
                var teacherName = teacher?.FullName ?? "Homeroom Teacher";

                // Create new assignment
                var studentSubject = new StudentSubject
                {
                    StudentId = studentId,
                    SubjectId = dto.SubjectId,
                    AssignedBy = teacherName,
                    AssignedDate = DateTime.UtcNow,
                    EnrolledDate = DateTime.UtcNow,
                    IsActive = true,
                    Notes = dto.Notes
                };

                _context.StudentSubjects.Add(studentSubject);
                await _context.SaveChangesAsync();

                // Get the subject details for response
                var subject = await _context.Subjects.FindAsync(dto.SubjectId);

                return new ApiResponse<StudentSubjectDto>
                {
                    Success = true,
                    Data = new StudentSubjectDto
                    {
                        Id = studentSubject.Id,
                        StudentId = studentSubject.StudentId,
                        SubjectId = studentSubject.SubjectId,
                        SubjectName = subject?.Name ?? "Unknown",
                        SubjectCode = subject?.Code ?? "",
                        EnrolledDate = studentSubject.EnrolledDate,
                        CompletedDate = studentSubject.CompletedDate,
                        IsActive = studentSubject.IsActive,
                        Notes = studentSubject.Notes,
                        AssignedBy = studentSubject.AssignedBy
                    },
                    Message = "Subject assigned successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<StudentSubjectDto>
                {
                    Success = false,
                    Message = $"Error assigning subject: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<object>> RemoveSubjectFromStudentAsync(int teacherId, int studentId, int subjectId, RemoveSubjectDto dto)
        {
            try
            {
                // Security check: Can this teacher access this student?
                if (!await CanAccessStudentAsync(teacherId, studentId))
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "You can only manage students in your homeroom"
                    };
                }

                var assignment = await _context.StudentSubjects
                    .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SubjectId == subjectId && ss.IsActive);

                if (assignment == null)
                {
                    return new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Subject assignment not found"
                    };
                }

                // Soft delete by setting IsActive to false
                assignment.IsActive = false;
                assignment.DroppedDate = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(dto.Reason))
                {
                    assignment.Notes = $"{assignment.Notes}\nRemoved: {dto.Reason}".Trim();
                }

                await _context.SaveChangesAsync();

                return new ApiResponse<object>
                {
                    Success = true,
                    Message = "Subject removed successfully"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error removing subject: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<HomeroomStudentDto>> UpdateStudentNameAsync(int teacherId, int studentId, UpdateStudentNameDto dto)
        {
            try
            {
                if (!await CanAccessStudentAsync(teacherId, studentId))
                {
                    return new ApiResponse<HomeroomStudentDto>
                    {
                        Success = false,
                        Message = "You can only update students in your homeroom"
                    };
                }

                var student = await _context.Students
                    .Include(s => s.Grade)
                    .FirstOrDefaultAsync(s => s.Id == studentId);
                if (student == null)
                {
                    return new ApiResponse<HomeroomStudentDto>
                    {
                        Success = false,
                        Message = "Student not found"
                    };
                }

                student.FirstName = dto.FirstName.Trim();
                student.MiddleName = string.IsNullOrWhiteSpace(dto.MiddleName) ? null : dto.MiddleName.Trim();
                student.LastName = dto.LastName.Trim();

                await _context.SaveChangesAsync();

                return new ApiResponse<HomeroomStudentDto>
                {
                    Success = true,
                    Message = "Student name updated successfully",
                    Data = new HomeroomStudentDto
                    {
                        Id = student.Id,
                        StudentNumber = student.StudentNumber,
                        FirstName = student.FirstName,
                        LastName = student.LastName,
                        FullName = student.FullName,
                        GradeName = student.Grade?.FullName ?? "",
                        GradeId = student.GradeId,
                        Subjects = new List<StudentSubjectDto>()
                    }
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<HomeroomStudentDto>
                {
                    Success = false,
                    Message = $"Error updating student name: {ex.Message}"
                };
            }
        }

        public async Task<ApiResponse<object>> BulkAssignSubjectsAsync(int teacherId, BulkAssignSubjectsDto dto)
        {
            try
            {
                var results = new List<string>();
                var successCount = 0;
                var errorCount = 0;

                foreach (var studentId in dto.StudentIds)
                {
                    // Security check for each student
                    if (!await CanAccessStudentAsync(teacherId, studentId))
                    {
                        results.Add($"Student {studentId}: Access denied - not in your homeroom");
                        errorCount++;
                        continue;
                    }

                    // Check if subject is already assigned
                    var existingAssignment = await _context.StudentSubjects
                        .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SubjectId == dto.SubjectId);

                    if (existingAssignment != null)
                    {
                        results.Add($"Student {studentId}: Subject already assigned");
                        errorCount++;
                        continue;
                    }

                    // Get teacher name for audit trail
                    var teacher = await _context.Users.FindAsync(teacherId);
                    var teacherName = teacher?.FullName ?? "Homeroom Teacher";

                    // Create new assignment
                    var studentSubject = new StudentSubject
                    {
                        StudentId = studentId,
                        SubjectId = dto.SubjectId,
                        AssignedBy = teacherName,
                        AssignedDate = DateTime.UtcNow,
                        EnrolledDate = DateTime.UtcNow,
                        IsActive = true,
                        Notes = dto.Notes
                    };

                    _context.StudentSubjects.Add(studentSubject);
                    successCount++;
                    results.Add($"Student {studentId}: Subject assigned successfully");
                }

                await _context.SaveChangesAsync();

                return new ApiResponse<object>
                {
                    Success = successCount > 0,
                    Data = new { 
                        SuccessCount = successCount, 
                        ErrorCount = errorCount, 
                        Results = results 
                    },
                    Message = $"Bulk assignment completed: {successCount} successful, {errorCount} errors"
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Error in bulk assignment: {ex.Message}"
                };
            }
        }
    }
}
