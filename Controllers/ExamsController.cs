using SchoolErpSMS.Data;
using SchoolErpSMS.DTOs;
using SchoolErpSMS.Entities;
using SchoolErpSMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace SchoolErpSMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ExamsController : ControllerBase
    {
        private readonly IExamService _examService;
        private readonly SchoolDbContext _context;
        private readonly ILogger<ExamsController> _logger;

        public ExamsController(IExamService examService, SchoolDbContext context, ILogger<ExamsController> logger)
        {
            _examService = examService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Admin: Get subject summary of teacher-entered marks for a grade/year/term (optional teacher filter)
        /// </summary>
        [HttpGet("admin/subject-summary")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<ActionResult<object>> GetAdminSubjectSummary(
            [FromQuery] int subjectId,
            [FromQuery] int gradeId,
            [FromQuery] int academicYear,
            [FromQuery] int term,
            [FromQuery] int? teacherId = null)
        {
            try
            {
                var subject = await _context.Subjects.FindAsync(subjectId);
                var grade = await _context.Grades.FindAsync(gradeId);

                var query = _context.ExamScores
                    .Include(s => s.Student)
                    .Include(s => s.ExamType)
                    .Include(s => s.Subject)
                    .Include(s => s.RecordedByTeacher)
                    .Where(s => s.SubjectId == subjectId &&
                                s.GradeId == gradeId &&
                                s.AcademicYear == academicYear &&
                                s.Term == term);

                if (teacherId.HasValue)
                {
                    query = query.Where(s => s.RecordedBy == teacherId.Value);
                }

                var scores = await query.ToListAsync();

                // Latest per student/exam type
                var latestByStudentExamType = scores
                    .GroupBy(s => new { s.StudentId, s.ExamTypeId })
                    .Select(g => g.OrderByDescending(x => x.RecordedAt).First())
                    .ToList();

                var result = latestByStudentExamType
                    .GroupBy(s => new { s.StudentId, StudentName = s.Student!.FullName })
                    .Select(g => new
                    {
                        studentId = g.Key.StudentId,
                        studentName = g.Key.StudentName,
                        entries = g.Select(e => new
                        {
                            examTypeId = e.ExamTypeId,
                            examTypeName = e.ExamType != null ? e.ExamType.Name : string.Empty,
                            score = e.IsAbsent ? 0m : e.Score,
                            isAbsent = e.IsAbsent,
                            recordedAt = e.RecordedAt,
                            recordedById = e.RecordedBy,
                            recordedByName = e.RecordedByTeacher != null ? e.RecordedByTeacher.FullName : null
                        })
                        .OrderBy(e => e.examTypeName)
                        .ToList()
                    })
                    .OrderBy(r => r.studentName)
                    .ToList();

                return Ok(new
                {
                    subjectId,
                    subjectName = subject?.Name,
                    gradeId,
                    gradeName = grade?.FullName ?? grade?.Name,
                    academicYear,
                    term,
                    teacherFilterId = teacherId,
                    totalStudents = result.Count,
                    totalEntries = latestByStudentExamType.Count,
                    students = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving admin subject summary");
                return StatusCode(500, "An error occurred while retrieving the subject summary");
            }
        }

        [HttpGet("student/{studentId}/term-summary")]
[Authorize(Roles = "Admin,Teacher,Staff")]
public async Task<ActionResult<IEnumerable<object>>> GetStudentTermSummary(
    int studentId,
    [FromQuery] int academicYear,
    [FromQuery] int term)
{
    try
    {
        var scores = await _context.ExamScores
            .Include(s => s.Subject)
            .Include(s => s.ExamType)
            .Where(s => s.StudentId == studentId &&
                        s.AcademicYear == academicYear &&
                        s.Term == term)
            .ToListAsync();

        string NormalizeType(string? name)
        {
            var n = (name ?? string.Empty).ToLowerInvariant();
            if (n.Contains("end") && n.Contains("term")) return "end";
            if (n.Contains("mid") || n.Contains("test 2") || n.Contains("test2")) return "mid";
            if (n.Contains("test 1") || n.Contains("test1") || n.Contains("t1")) return "test1";
            return string.Empty;
        }

        var latestByType = scores
            .GroupBy(s => new { s.SubjectId, Kind = NormalizeType(s.ExamType!.Name) })
            .Select(g => g
                .OrderByDescending(x => x.RecordedAt)
                .First())
            .ToList();

        var bySubject = latestByType
            .GroupBy(s => new { s.SubjectId, SubjectName = s.Subject!.Name })
            .Select(g =>
            {
                var test1 = g.FirstOrDefault(x => NormalizeType(x.ExamType!.Name) == "test1");
                var mid = g.FirstOrDefault(x => NormalizeType(x.ExamType!.Name) == "mid");
                var end = g.FirstOrDefault(x => NormalizeType(x.ExamType!.Name) == "end");
                decimal v(decimal? d, bool absent) => absent ? 0m : (d ?? 0m);

                return new
                {
                    subjectId = g.Key.SubjectId,
                    subjectName = g.Key.SubjectName,
                    test1Score = test1 != null ? v(test1.Score, test1.IsAbsent) : 0m,
                    midTermScore = mid != null ? v(mid.Score, mid.IsAbsent) : 0m,
                    endTermScore = end != null ? v(end.Score, end.IsAbsent) : 0m
                };
            })
            .OrderBy(r => r.subjectName)
            .ToList();

        return Ok(bySubject);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving term summary for StudentId: {StudentId}", studentId);
        return StatusCode(500, "An error occurred while retrieving term summary");
    }
}

        /// <summary>
        /// Get exam scores for a student
        /// </summary>
        [HttpGet("student/{studentId}/scores")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<ExamScoreDto>>> GetStudentScores(int studentId, [FromQuery] int academicYear, [FromQuery] int term)
        {
            try
            {
                var scores = await _examService.GetScoresByStudentAsync(studentId, academicYear, term);
                var scoreDtos = scores.Select(s => new ExamScoreDto
                {
                    Id = s.Id,
                    StudentId = s.StudentId,
                    StudentName = s.Student?.FullName,
                    SubjectId = s.SubjectId,
                    SubjectName = s.Subject?.Name,
                    ExamTypeId = s.ExamTypeId,
                    ExamTypeName = s.ExamType?.Name,
                    Score = s.Score,
                    IsAbsent = s.IsAbsent, 
                    AcademicYear = s.AcademicYear,
                    Term = s.Term,
                    RecordedAt = s.RecordedAt,
                    RecordedByName = s.RecordedByTeacher?.FullName,
                    // Add comment fields
                    Comments = s.Comments,
                    CommentsUpdatedAt = s.CommentsUpdatedAt,
                    CommentsUpdatedByName = s.CommentsUpdatedByTeacher?.FullName
                });

                return Ok(scoreDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student scores for StudentId: {StudentId}", studentId);
                return StatusCode(500, "An error occurred while retrieving student scores");
            }
        }

        /// <summary>
        /// Get exam scores for a grade/class
        /// </summary>
        [HttpGet("grade/{gradeId}/scores")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<ExamScoreDto>>> GetGradeScores(int gradeId, [FromQuery] int academicYear, [FromQuery] int term)
        {
            try
            {
                var scores = await _examService.GetScoresByGradeAsync(gradeId, academicYear, term);
                var scoreDtos = scores.Select(s => new ExamScoreDto
                {
                    Id = s.Id,
                    StudentId = s.StudentId,
                    StudentName = s.Student?.FullName,
                    SubjectId = s.SubjectId,
                    SubjectName = s.Subject?.Name,
                    ExamTypeId = s.ExamTypeId,
                    ExamTypeName = s.ExamType?.Name,
                    Score = s.Score,
                    IsAbsent = s.IsAbsent, 
                    AcademicYear = s.AcademicYear,
                    Term = s.Term,
                    RecordedAt = s.RecordedAt,
                    RecordedByName = s.RecordedByTeacher?.FullName,
                    // Add comment fields
                    Comments = s.Comments,
                    CommentsUpdatedAt = s.CommentsUpdatedAt,
                    CommentsUpdatedByName = s.CommentsUpdatedByTeacher?.FullName

                });

                return Ok(scoreDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving grade scores for GradeId: {GradeId}", gradeId);
                return StatusCode(500, "An error occurred while retrieving grade scores");
            }
        }

        /// <summary>
        /// Get grade-wide per-subject Test1/Mid/End summary for each student in a term
        /// </summary>
        [HttpGet("grade/{gradeId}/term-summary")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<object>>> GetGradeTermSummary(
            int gradeId,
            [FromQuery] int academicYear,
            [FromQuery] int term)
        {
            try
            {
                var scores = await _context.ExamScores
                    .Include(s => s.Student)
                    .Include(s => s.Subject)
                    .Include(s => s.ExamType)
                    .Where(s => s.GradeId == gradeId &&
                               s.AcademicYear == academicYear &&
                               s.Term == term)
                    .ToListAsync();

                string NormalizeType(string? name)
                {
                    var n = (name ?? string.Empty).ToLowerInvariant();
                    if (n.Contains("end") && n.Contains("term")) return "end";
                    if (n.Contains("mid") || n.Contains("test 2") || n.Contains("test2")) return "mid";
                    if (n.Contains("test 1") || n.Contains("test1") || n.Contains("t1")) return "test1";
                    return string.Empty;
                }

                // For each student+subject+exam-kind, pick latest by RecordedAt
                var latestByType = scores
                    .GroupBy(s => new { s.StudentId, s.SubjectId, Kind = NormalizeType(s.ExamType!.Name) })
                    .Select(g => g
                        .OrderByDescending(x => x.RecordedAt)
                        .First())
                    .ToList();

                decimal ValueOrZero(decimal? d, bool absent) => absent ? 0m : (d ?? 0m);

                var result = latestByType
                    .GroupBy(s => new { s.StudentId, StudentName = s.Student!.FullName, s.SubjectId, SubjectName = s.Subject!.Name })
                    .Select(g =>
                    {
                        var test1 = g.FirstOrDefault(x => NormalizeType(x.ExamType!.Name) == "test1");
                        var mid = g.FirstOrDefault(x => NormalizeType(x.ExamType!.Name) == "mid");
                        var end = g.FirstOrDefault(x => NormalizeType(x.ExamType!.Name) == "end");

                        return new
                        {
                            studentId = g.Key.StudentId,
                            studentName = g.Key.StudentName,
                            subjectId = g.Key.SubjectId,
                            subjectName = g.Key.SubjectName,
                            test1Score = test1 != null ? ValueOrZero(test1.Score, test1.IsAbsent) : 0m,
                            midTermScore = mid != null ? ValueOrZero(mid.Score, mid.IsAbsent) : 0m,
                            endTermScore = end != null ? ValueOrZero(end.Score, end.IsAbsent) : 0m
                        };
                    })
                    .OrderBy(r => r.studentName)
                    .ThenBy(r => r.subjectName)
                    .ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving term summary for GradeId: {GradeId}", gradeId);
                return StatusCode(500, "An error occurred while retrieving grade term summary");
            }
        }

        /// <summary>
        /// Get a teacher-specific summary of scores they entered for a subject in a grade for a term
        /// </summary>
        [HttpGet("teacher/subject-summary")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<object>> GetTeacherSubjectSummary(
            [FromQuery] int subjectId,
            [FromQuery] int gradeId,
            [FromQuery] int academicYear,
            [FromQuery] int term)
        {
            try
            {
                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim) || !int.TryParse(teacherIdClaim, out int teacherId))
                {
                    return Unauthorized("Invalid teacher credentials");
                }

                // Verify the teacher is assigned to this subject and grade (authorization)
                var canEnter = await _examService.CanTeacherEnterScore(teacherId, subjectId, gradeId);
                if (!canEnter)
                {
                    return Forbid("You are not assigned to this subject in this grade");
                }

                var scores = await _context.ExamScores
                    .Include(s => s.Student)
                    .Include(s => s.ExamType)
                    .Include(s => s.Subject)
                    .Include(s => s.RecordedByTeacher)
                    .Where(s => s.SubjectId == subjectId &&
                                s.GradeId == gradeId &&
                                s.AcademicYear == academicYear &&
                                s.Term == term &&
                                s.RecordedBy == teacherId)
                    .ToListAsync();

                // Latest entry per student per exam type (from this teacher)
                var latestByStudentExamType = scores
                    .GroupBy(s => new { s.StudentId, s.ExamTypeId })
                    .Select(g => g.OrderByDescending(x => x.RecordedAt).First())
                    .ToList();

                var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Id == gradeId);
                var subject = await _context.Subjects.FirstOrDefaultAsync(sb => sb.Id == subjectId);

                var result = latestByStudentExamType
                    .GroupBy(s => new { s.StudentId, StudentName = s.Student!.FullName })
                    .Select(g => new
                    {
                        studentId = g.Key.StudentId,
                        studentName = g.Key.StudentName,
                        entries = g.Select(e => new
                        {
                            examTypeId = e.ExamTypeId,
                            examTypeName = e.ExamType!.Name,
                            score = e.IsAbsent ? 0m : e.Score,
                            isAbsent = e.IsAbsent,
                            recordedAt = e.RecordedAt
                        })
                        .OrderBy(entry => entry.examTypeName)
                        .ToList()
                    })
                    .OrderBy(r => r.studentName)
                    .ToList();

                return Ok(new
                {
                    subjectId,
                    subjectName = subject?.Name,
                    gradeId,
                    gradeName = grade?.FullName ?? grade?.Name,
                    academicYear,
                    term,
                    teacherId,
                    teacherName = scores.FirstOrDefault()?.RecordedByTeacher?.FullName,
                    totalStudents = result.Count,
                    totalEntries = latestByStudentExamType.Count,
                    students = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teacher subject summary for SubjectId: {SubjectId}, GradeId: {GradeId}", subjectId, gradeId);
                return StatusCode(500, "An error occurred while retrieving teacher subject summary");
            }
        }

        /// <summary>
        /// Get students by grade - NEW ENDPOINT needed by frontend
        /// </summary>
        [HttpGet("students/grade/{gradeId}")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsByGrade(int gradeId)
        {
            try
            {
                var students = await _context.Students
                    .Where(s => s.GradeId == gradeId && s.IsActive)
                    .OrderBy(s => s.FullName)
                    .Select(s => new StudentDto
                    {
                        Id = s.Id,
                        FullName = s.FullName,
                        StudentNumber = s.StudentNumber,
                        GradeId = s.GradeId
                    })
                    .ToListAsync();

                return Ok(students);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students for GradeId: {GradeId}", gradeId);
                return StatusCode(500, "An error occurred while retrieving students");
            }
        }

        /// <summary>
        /// Export gradebook for a grade/subject - NEW ENDPOINT needed by frontend
        /// </summary>
        [HttpGet("grade/{gradeId}/export")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult> ExportGradeBook(
            int gradeId, 
            [FromQuery] int academicYear, 
            [FromQuery] int term,
            [FromQuery] int? subjectId = null,
            [FromQuery] string format = "xlsx")
        {
            try
            {
                // Get scores for the grade
                var scoresQuery = _context.ExamScores
                    .Include(s => s.Student)
                    .Include(s => s.Subject)
                    .Include(s => s.ExamType)
                    .Where(s => s.GradeId == gradeId && 
                               s.AcademicYear == academicYear && 
                               s.Term == term);

                if (subjectId.HasValue)
                {
                    scoresQuery = scoresQuery.Where(s => s.SubjectId == subjectId.Value);
                }

                var scores = await scoresQuery.ToListAsync();

                // Get all students in grade for complete roster
                var students = await _context.Students
                    .Where(s => s.GradeId == gradeId && s.IsActive)
                    .OrderBy(s => s.FullName)
                    .ToListAsync();

                // Create CSV content
                var csv = new StringBuilder();
                
                // Headers
                csv.AppendLine("Student Number,Student Name,Subject,Exam Type,Score,Grade,Recorded Date");

                // Group scores by student
                var studentScores = scores.GroupBy(s => s.StudentId).ToDictionary(g => g.Key, g => g.ToList());

                foreach (var student in students)
                {
                    var studentScoresList = studentScores.ContainsKey(student.Id) ? studentScores[student.Id] : new List<ExamScore>();
                    
                    if (studentScoresList.Any())
                    {
                        foreach (var score in studentScoresList)
                        {
                            var grade = CalculateGrade(score.Score);
                            csv.AppendLine($"{student.StudentNumber},{student.FullName},{score.Subject?.Name},{score.ExamType?.Name},{score.Score},{grade},{score.RecordedAt:yyyy-MM-dd}");
                        }
                    }
                    else
                    {
                        // Include students with no scores
                        csv.AppendLine($"{student.StudentNumber},{student.FullName},,,,,");
                    }
                }

                var fileName = $"gradebook_{gradeId}_{academicYear}_T{term}.csv";
                var bytes = Encoding.UTF8.GetBytes(csv.ToString());

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting gradebook for GradeId: {GradeId}", gradeId);
                return StatusCode(500, "An error occurred while exporting gradebook");
            }
        }

        /// <summary>
        /// Generate report card for a student - NEW ENDPOINT
        /// </summary>
        [HttpGet("student/{studentId}/report-card")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult> GenerateReportCard(int studentId, [FromQuery] int academicYear, [FromQuery] int term)
        {
            try
            {
                var student = await _context.Students
                    .Include(s => s.Grade)
                    .FirstOrDefaultAsync(s => s.Id == studentId);

                if (student == null)
                {
                    return NotFound("Student not found");
                }

                var scores = await _context.ExamScores
                    .Include(s => s.Subject)
                    .Include(s => s.ExamType)
                    .Where(s => s.StudentId == studentId && 
                               s.AcademicYear == academicYear && 
                               s.Term == term)
                    .ToListAsync();

                // Create simple CSV report card
                var csv = new StringBuilder();
                csv.AppendLine($"REPORT CARD");
                csv.AppendLine($"Student: {student.FullName}");
                csv.AppendLine($"Student Number: {student.StudentNumber}");
                csv.AppendLine($"Grade: {student.Grade?.FullName}");
                csv.AppendLine($"Academic Year: {academicYear}");
                csv.AppendLine($"Term: {term}");
                csv.AppendLine();
                csv.AppendLine("Subject,Exam Type,Score,Grade");

                foreach (var score in scores.OrderBy(s => s.Subject?.Name).ThenBy(s => s.ExamType?.Name))
                {
                    var grade = CalculateGrade(score.Score);
                    csv.AppendLine($"{score.Subject?.Name},{score.ExamType?.Name},{score.Score},{grade}");
                }

                var fileName = $"report_card_{student.StudentNumber}_{academicYear}_T{term}.csv";
                var bytes = Encoding.UTF8.GetBytes(csv.ToString());

                return File(bytes, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report card for StudentId: {StudentId}", studentId);
                return StatusCode(500, "An error occurred while generating report card");
            }
        }



        /// <summary>
        /// Bulk submit scores - NEW ENDPOINT
        /// </summary>
        [HttpPost("scores/bulk")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> BulkSubmitScores([FromBody] List<CreateExamScoreDto> scoresData)
        {
            try
            {
                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim) || !int.TryParse(teacherIdClaim, out int teacherId))
                {
                    return Unauthorized("Invalid teacher credentials");
                }

                var results = new List<ExamScoreDto>();

                foreach (var scoreDto in scoresData)
                {
                    // Get student's grade
                    var student = await _context.Students.FindAsync(scoreDto.StudentId);
                    if (student == null) continue;

                    // Check authorization with student-specific check for optional subjects
                    var canEnter = await _examService.CanTeacherEnterScoreForStudent(teacherId, scoreDto.SubjectId, student.GradeId, scoreDto.StudentId);
                    if (!canEnter) continue;

                    var examScore = new ExamScore
                    {
                        StudentId = scoreDto.StudentId,
                        SubjectId = scoreDto.SubjectId,
                        ExamTypeId = scoreDto.ExamTypeId,
                        GradeId = student.GradeId,
                        Score = scoreDto.Score,
                        AcademicYear = scoreDto.AcademicYear,
                        Term = scoreDto.Term,
                        RecordedBy = teacherId,
                        Comments = scoreDto.Comments,
                        CommentsUpdatedAt = !string.IsNullOrWhiteSpace(scoreDto.Comments) ? DateTime.UtcNow : null,
                        CommentsUpdatedBy = !string.IsNullOrWhiteSpace(scoreDto.Comments) ? teacherId : null
                    };

                    var createdScore = await _examService.CreateOrUpdateScoreAsync(examScore);
                    
                    // Add to results
                    results.Add(new ExamScoreDto
                    {
                        Id = createdScore.Id,
                        StudentId = createdScore.StudentId,
                        SubjectId = createdScore.SubjectId,
                        ExamTypeId = createdScore.ExamTypeId,
                        Score = createdScore.Score,
                        AcademicYear = createdScore.AcademicYear,
                        Term = createdScore.Term,
                        RecordedAt = createdScore.RecordedAt,
                        Comments = createdScore.Comments,
                        CommentsUpdatedAt = createdScore.CommentsUpdatedAt
                    });
                }

                return Ok(new { Message = $"Successfully processed {results.Count} scores", Results = results });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in bulk submit scores");
                return StatusCode(500, "An error occurred while processing bulk scores");
            }
        }

        /// <summary>
        /// Update existing score - NEW ENDPOINT
        /// </summary>
    [HttpPut("scores/{scoreId}")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<ExamScoreDto>> UpdateScore(int scoreId, [FromBody] UpdateExamScoreDto scoreDto)
    {
        try
        {
            _logger.LogInformation("UpdateScore called for ScoreId: {ScoreId} by user: {UserId}", scoreId, User?.Identity?.Name);

            // Validate input
            if (scoreDto == null)
            {
                _logger.LogWarning("UpdateScore called with null scoreDto for ScoreId: {ScoreId}", scoreId);
                return BadRequest("Score data is required");
            }

            var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherIdClaim) || !int.TryParse(teacherIdClaim, out int teacherId))
            {
                _logger.LogError("Could not parse teacher ID from claims. Claim value: {ClaimValue}", teacherIdClaim);
                return Unauthorized("Invalid teacher credentials");
            }

            var existingScore = await _context.ExamScores
                .Include(s => s.Student)
                .Include(s => s.Subject)
                .Include(s => s.ExamType)
                .Include(s => s.RecordedByTeacher)
                .Include(s => s.CommentsUpdatedByTeacher)
                .FirstOrDefaultAsync(s => s.Id == scoreId);

            if (existingScore == null)
            {
                _logger.LogWarning("Score not found with ID: {ScoreId}", scoreId);
                return NotFound("Score not found");
            }

            _logger.LogInformation("Found existing score for Student: {StudentName}, Subject: {SubjectName}", 
                existingScore.Student?.FullName, existingScore.Subject?.Name);

            // Check authorization with student-specific check for optional subjects
            var canEnter = await _examService.CanTeacherEnterScoreForStudent(teacherId, existingScore.SubjectId, existingScore.Student.GradeId, existingScore.StudentId);
            if (!canEnter)
            {
                _logger.LogWarning("Teacher {TeacherId} not authorized to update score {ScoreId} for SubjectId: {SubjectId}, GradeId: {GradeId}, StudentId: {StudentId}", 
                    teacherId, scoreId, existingScore.SubjectId, existingScore.Student.GradeId, existingScore.StudentId);
                return Forbid("Not authorized to update this score");
            }

            // Track if comments were updated
            bool commentsUpdated = false;

            // Update score and absent status
            if (existingScore.Score != scoreDto.Score || existingScore.IsAbsent != scoreDto.IsAbsent)
            {
                _logger.LogInformation("Updating score from {OldScore} to {NewScore} and absent from {OldAbsent} to {NewAbsent} for ScoreId: {ScoreId}", 
                    existingScore.Score, scoreDto.Score, existingScore.IsAbsent, scoreDto.IsAbsent, scoreId);
                existingScore.Score = scoreDto.Score;
                existingScore.IsAbsent = scoreDto.IsAbsent;
                existingScore.RecordedAt = DateTime.UtcNow;
                existingScore.RecordedBy = teacherId;
            }

            // Update comments if provided and different from existing
            if (scoreDto.Comments != existingScore.Comments)
            {
                _logger.LogInformation("Updating comments for ScoreId: {ScoreId}", scoreId);
                existingScore.Comments = scoreDto.Comments;
                existingScore.CommentsUpdatedAt = DateTime.UtcNow;
                existingScore.CommentsUpdatedBy = teacherId;
                commentsUpdated = true;
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Score {ScoreId} updated successfully", scoreId);

            // Reload to get updated navigation properties
            await _context.Entry(existingScore)
                .Reference(s => s.CommentsUpdatedByTeacher)
                .LoadAsync();

            var responseDto = new ExamScoreDto
            {
                Id = existingScore.Id,
                StudentId = existingScore.StudentId,
                StudentName = existingScore.Student?.FullName,
                SubjectId = existingScore.SubjectId,
                SubjectName = existingScore.Subject?.Name,
                ExamTypeId = existingScore.ExamTypeId,
                ExamTypeName = existingScore.ExamType?.Name,
                Score = existingScore.Score,
                IsAbsent = existingScore.IsAbsent,
                AcademicYear = existingScore.AcademicYear,
                Term = existingScore.Term,
                RecordedAt = existingScore.RecordedAt,
                RecordedByName = existingScore.RecordedByTeacher?.FullName,
                Comments = existingScore.Comments,
                CommentsUpdatedAt = existingScore.CommentsUpdatedAt,
                CommentsUpdatedByName = existingScore.CommentsUpdatedByTeacher?.FullName
            };

            _logger.LogInformation("Returning updated score response: {@ResponseDto}", responseDto);
            return Ok(responseDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, "Invalid argument in UpdateScore for ScoreId: {ScoreId}", scoreId);
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unauthorized access in UpdateScore for ScoreId: {ScoreId}", scoreId);
            return Unauthorized("Access denied");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating score {ScoreId}", scoreId);
            return StatusCode(500, "An unexpected error occurred while updating the score");
        }
    }

        /// <summary>
        /// Toggle absent status for a score (using comments field)
        /// </summary>
        [HttpPatch("scores/{scoreId}/toggle-absent")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult<ExamScoreDto>> ToggleAbsentStatus(int scoreId)
        {
            try
            {
                _logger.LogInformation("ToggleAbsentStatus called for ScoreId: {ScoreId} by user: {UserId}", scoreId, User?.Identity?.Name);

                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim) || !int.TryParse(teacherIdClaim, out int teacherId))
                {
                    _logger.LogError("Could not parse teacher ID from claims. Claim value: {ClaimValue}", teacherIdClaim);
                    return Unauthorized("Invalid teacher credentials");
                }

                var existingScore = await _context.ExamScores
                    .Include(s => s.Student)
                    .Include(s => s.Subject)
                    .Include(s => s.ExamType)
                    .Include(s => s.RecordedByTeacher)
                    .Include(s => s.CommentsUpdatedByTeacher)
                    .FirstOrDefaultAsync(s => s.Id == scoreId);

                if (existingScore == null)
                {
                    _logger.LogWarning("Score not found with ID: {ScoreId}", scoreId);
                    return NotFound("Score not found");
                }

                // Check authorization
                var canEnter = await _examService.CanTeacherEnterScore(teacherId, existingScore.SubjectId, existingScore.Student.GradeId);
                if (!canEnter)
                {
                    _logger.LogWarning("Teacher {TeacherId} not authorized to update score {ScoreId}", teacherId, scoreId);
                    return Forbid("Not authorized to update this score");
                }

                // Toggle absent status using helper method
                var currentlyAbsent = _examService.IsStudentAbsent(existingScore);
                _examService.MarkAsAbsent(existingScore, !currentlyAbsent);
                
                existingScore.RecordedAt = DateTime.UtcNow;
                existingScore.RecordedBy = teacherId;

                await _context.SaveChangesAsync();
                _logger.LogInformation("Absent status toggled to {IsAbsent} for ScoreId: {ScoreId}", existingScore.IsAbsent, scoreId);

                var responseDto = new ExamScoreDto
                {
                    Id = existingScore.Id,
                    StudentId = existingScore.StudentId,
                    StudentName = existingScore.Student?.FullName,
                    SubjectId = existingScore.SubjectId,
                    SubjectName = existingScore.Subject?.Name,
                    ExamTypeId = existingScore.ExamTypeId,
                                    ExamTypeName = existingScore.ExamType?.Name,
                Score = existingScore.Score,
                IsAbsent = existingScore.IsAbsent,
                AcademicYear = existingScore.AcademicYear,
                    Term = existingScore.Term,
                    RecordedAt = existingScore.RecordedAt,
                    RecordedByName = existingScore.RecordedByTeacher?.FullName,
                    Comments = existingScore.Comments,
                    CommentsUpdatedAt = existingScore.CommentsUpdatedAt,
                    CommentsUpdatedByName = existingScore.CommentsUpdatedByTeacher?.FullName
                };

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error toggling absent status for score {ScoreId}", scoreId);
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Bulk mark students as absent for a specific exam
        /// </summary>
        [HttpPost("scores/bulk-absent")]
        [Authorize(Roles = "Teacher")]
        public async Task<ActionResult> BulkMarkAbsent([FromBody] BulkAbsentDto request)
        {
            try
            {
                _logger.LogInformation("BulkMarkAbsent called by user: {UserId}", User?.Identity?.Name);

                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim) || !int.TryParse(teacherIdClaim, out int teacherId))
                {
                    _logger.LogError("Could not parse teacher ID from claims. Claim value: {ClaimValue}", teacherIdClaim);
                    return Unauthorized("Invalid teacher credentials");
                }

                if (request == null || !request.StudentIds.Any())
                {
                    return BadRequest("Student IDs are required");
                }

                // Get all scores for the specified exam type, subject, grade, academic year, and term
                var scores = await _context.ExamScores
                    .Include(s => s.Student)
                    .Where(s => s.ExamTypeId == request.ExamTypeId &&
                               s.SubjectId == request.SubjectId &&
                               s.AcademicYear == request.AcademicYear &&
                               s.Term == request.Term &&
                               request.StudentIds.Contains(s.StudentId))
                    .ToListAsync();

                if (!scores.Any())
                {
                    return NotFound("No scores found for the specified criteria");
                }

                // Check authorization for the subject and grade
                var gradeId = scores.First().Student.GradeId;
                var canEnter = await _examService.CanTeacherEnterScore(teacherId, request.SubjectId, gradeId);
                if (!canEnter)
                {
                    _logger.LogWarning("Teacher {TeacherId} not authorized to update scores for SubjectId: {SubjectId}, GradeId: {GradeId}", 
                        teacherId, request.SubjectId, gradeId);
                    return Forbid("Not authorized to update these scores");
                }

                var updatedCount = 0;
                foreach (var score in scores)
                {
                    score.IsAbsent = request.IsAbsent;
                    score.Score = request.IsAbsent ? 0 : score.Score; // Set score to 0 if marking as absent
                    score.RecordedAt = DateTime.UtcNow;
                    score.RecordedBy = teacherId;
                    updatedCount++;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Bulk marked {UpdatedCount} scores as absent={IsAbsent} for SubjectId: {SubjectId}, ExamTypeId: {ExamTypeId}", 
                    updatedCount, request.IsAbsent, request.SubjectId, request.ExamTypeId);

                return Ok(new { 
                    Message = $"Successfully marked {updatedCount} students as {(request.IsAbsent ? "absent" : "present")}",
                    UpdatedCount = updatedCount 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in bulk mark absent");
                return StatusCode(500, "An unexpected error occurred");
            }
        }

        /// <summary>
        /// Delete score - NEW ENDPOINT
        /// </summary>
        [HttpDelete("scores/{scoreId}")]
        [Authorize(Roles = "Teacher,Admin")]
        public async Task<ActionResult> DeleteScore(int scoreId)
        {
            try
            {
                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim) || !int.TryParse(teacherIdClaim, out int teacherId))
                {
                    return Unauthorized("Invalid teacher credentials");
                }

                var existingScore = await _context.ExamScores
                    .Include(s => s.Student)
                    .FirstOrDefaultAsync(s => s.Id == scoreId);

                if (existingScore == null)
                {
                    return NotFound("Score not found");
                }

                // Check authorization (teachers can only delete their own scores)
                if (!User.IsInRole("Admin") && existingScore.RecordedBy != teacherId)
                {
                    return Forbid("Not authorized to delete this score");
                }

                _context.ExamScores.Remove(existingScore);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Score deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting score {ScoreId}", scoreId);
                return StatusCode(500, "An error occurred while deleting the score");
            }
        }

        /// <summary>
        /// Get score statistics - NEW ENDPOINT
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<object>> GetScoreStatistics(
            [FromQuery] int? gradeId = null,
            [FromQuery] int? subjectId = null,
            [FromQuery] int? academicYear = null,
            [FromQuery] int? term = null)
        {
            try
            {
                var query = _context.ExamScores.AsQueryable();

                if (gradeId.HasValue) query = query.Where(s => s.GradeId == gradeId);
                if (subjectId.HasValue) query = query.Where(s => s.SubjectId == subjectId);
                if (academicYear.HasValue) query = query.Where(s => s.AcademicYear == academicYear);
                if (term.HasValue) query = query.Where(s => s.Term == term);

                var scores = await query.Select(s => s.Score).ToListAsync();

                if (!scores.Any())
                {
                    return Ok(new { Message = "No scores found for the specified criteria" });
                }

                var statistics = new
                {
                    Count = scores.Count,
                    Average = scores.Average(),
                    Minimum = scores.Min(),
                    Maximum = scores.Max(),
                    PassingRate = scores.Count(s => s >= 60) * 100.0 / scores.Count,
                    GradeDistribution = new
                    {
                        A = scores.Count(s => s >= 90),
                        B = scores.Count(s => s >= 80 && s < 90),
                        C = scores.Count(s => s >= 70 && s < 80),
                        D = scores.Count(s => s >= 60 && s < 70),
                        F = scores.Count(s => s < 60)
                    }
                };

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating score statistics");
                return StatusCode(500, "An error occurred while calculating statistics");
            }
        }

        /// <summary>
        /// Check if teacher can enter score for subject/grade - NEW ENDPOINT
        /// </summary>
        [HttpGet("teacher/{teacherId}/can-enter-score")]
        [Authorize(Roles = "Admin,Teacher")]
        public async Task<ActionResult<bool>> CanTeacherEnterScore(int teacherId, [FromQuery] int subjectId, [FromQuery] int gradeId)
        {
            try
            {
                var canEnter = await _examService.CanTeacherEnterScore(teacherId, subjectId, gradeId);
                return Ok(canEnter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking teacher authorization");
                return StatusCode(500, "An error occurred while checking authorization");
            }
        }

        /// <summary>
        /// Get students enrolled in a teacher's optional subject
        /// </summary>
        [HttpGet("optional-subject/{subjectId}/students")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetStudentsForOptionalSubject(int subjectId, [FromQuery] int gradeId)
        {
            try
            {
                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim) || !int.TryParse(teacherIdClaim, out int teacherId))
                {
                    return Unauthorized("Invalid teacher credentials");
                }

                // Check if teacher is assigned to this subject in this grade
                var teacherAssigned = await _examService.CanTeacherEnterScore(teacherId, subjectId, gradeId);
                if (!teacherAssigned)
                {
                    return Forbid("You are not assigned to teach this subject in this grade");
                }

                // Check if this is an optional subject
                var isOptionalSubject = await _context.GradeSubjects
                    .AnyAsync(gs => gs.SubjectId == subjectId 
                                 && gs.GradeId == gradeId 
                                 && gs.IsOptional 
                                 && gs.IsActive);

                if (!isOptionalSubject)
                {
                    return BadRequest("This is not an optional subject");
                }

                // Get students enrolled in this optional subject
                var enrolledStudents = await _context.StudentOptionalSubjects
                    .Include(sos => sos.Student)
                        .ThenInclude(s => s.Grade)
                    .Where(sos => sos.SubjectId == subjectId)
                    .Select(sos => sos.Student)
                    .Where(s => s.GradeId == gradeId && s.IsActive && !s.IsArchived)
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
                        FullName = s.FullName
                    })
                    .ToListAsync();

                return Ok(enrolledStudents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting students for optional subject {SubjectId} in grade {GradeId}", subjectId, gradeId);
                return StatusCode(500, "An error occurred while getting students for optional subject");
            }
        }

        /// <summary>
        /// Create or update exam score (Teachers only)
        /// </summary>
        [HttpPost("scores")]
        [Authorize] // Remove role restriction temporarily
        public async Task<ActionResult<ExamScoreDto>> CreateOrUpdateScore([FromBody] CreateExamScoreDto scoreDto)
        {
            try
        {
        _logger.LogInformation("CreateOrUpdateScore called by user: {UserId}", User?.Identity?.Name);
        _logger.LogInformation("User authenticated: {IsAuthenticated}", User?.Identity?.IsAuthenticated);
        _logger.LogInformation("User role claims: {Claims}", string.Join(", ", User?.Claims?.Select(c => $"{c.Type}:{c.Value}") ?? new string[0]));

        // Manual role check since [Authorize(Roles = "Teacher")] isn't working
        var hasTeacherRole = User?.Claims?.Any(c => 
            (c.Type == ClaimTypes.Role || c.Type == "role") && 
            c.Value.Equals("Teacher", StringComparison.OrdinalIgnoreCase)) ?? false;

        if (!hasTeacherRole)
        {
            _logger.LogWarning("User does not have Teacher role. Claims: {Claims}", 
                string.Join(", ", User?.Claims?.Select(c => $"{c.Type}:{c.Value}") ?? new string[0]));
            return StatusCode(403, "Teacher role required");
        }

        // Validate input
        if (scoreDto == null)
        {
            _logger.LogWarning("CreateOrUpdateScore called with null scoreDto");
            return BadRequest("Score data is required");
        }

        // Get teacher ID from claims with better error handling
        var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(teacherIdClaim) || !int.TryParse(teacherIdClaim, out int teacherId))
        {
            _logger.LogError("Could not parse teacher ID from claims. Claim value: {ClaimValue}", teacherIdClaim);
            return Unauthorized("Invalid teacher credentials");
        }

        _logger.LogInformation("Processing score entry for TeacherId: {TeacherId}, StudentId: {StudentId}, SubjectId: {SubjectId}", 
            teacherId, scoreDto.StudentId, scoreDto.SubjectId);

        // Get student's grade to check authorization
        var student = await _context.Students.FindAsync(scoreDto.StudentId);
        if (student == null)
        {
            _logger.LogWarning("Student not found with ID: {StudentId}", scoreDto.StudentId);
            return NotFound("Student not found");
        }

        _logger.LogInformation("Found student: {StudentName}, GradeId: {GradeId}", student.FullName, student.GradeId);

            // Check if teacher can enter score for this subject/grade/student (includes optional subject enrollment check)
            var canEnter = await _examService.CanTeacherEnterScoreForStudent(teacherId, scoreDto.SubjectId, student.GradeId, scoreDto.StudentId);
            if (!canEnter)
            {
                _logger.LogWarning("Teacher {TeacherId} not authorized to enter scores for SubjectId: {SubjectId}, GradeId: {GradeId}, StudentId: {StudentId}", 
                    teacherId, scoreDto.SubjectId, student.GradeId, scoreDto.StudentId);
                return StatusCode(403, "You are not authorized to enter scores for this subject, grade, and student combination");
            }

        var examScore = new ExamScore
        {
            StudentId = scoreDto.StudentId,
            SubjectId = scoreDto.SubjectId,
            ExamTypeId = scoreDto.ExamTypeId,
            GradeId = student.GradeId,
            Score = scoreDto.Score,
            IsAbsent = scoreDto.IsAbsent,
            AcademicYear = scoreDto.AcademicYear,
            Term = scoreDto.Term,
            RecordedBy = teacherId,
            // Handle comments
            Comments = scoreDto.Comments,
            CommentsUpdatedAt = !string.IsNullOrWhiteSpace(scoreDto.Comments) ? DateTime.UtcNow : null,
            CommentsUpdatedBy = !string.IsNullOrWhiteSpace(scoreDto.Comments) ? teacherId : null
        };

        _logger.LogInformation("Creating/updating exam score: {@ExamScore}", examScore);

        var createdScore = await _examService.CreateOrUpdateScoreAsync(examScore);

        _logger.LogInformation("Score created/updated successfully with ID: {ScoreId}", createdScore.Id);

        // Reload the entity with navigation properties for proper response
        var scoreWithNavigation = await _context.ExamScores
            .Include(s => s.Student)
            .Include(s => s.Subject)
            .Include(s => s.ExamType)
            .Include(s => s.RecordedByTeacher)
            .Include(s => s.CommentsUpdatedByTeacher) // Include comment author
            .FirstOrDefaultAsync(s => s.Id == createdScore.Id);

        if (scoreWithNavigation == null)
        {
            _logger.LogError("Score was created but could not be retrieved with ID: {ScoreId}", createdScore.Id);
            return StatusCode(500, "Score was created but could not be retrieved");
        }

        var responseDto = new ExamScoreDto
        {
            Id = scoreWithNavigation.Id,
            StudentId = scoreWithNavigation.StudentId,
            StudentName = scoreWithNavigation.Student?.FullName,
            SubjectId = scoreWithNavigation.SubjectId,
            SubjectName = scoreWithNavigation.Subject?.Name,
            ExamTypeId = scoreWithNavigation.ExamTypeId,
            ExamTypeName = scoreWithNavigation.ExamType?.Name,
            Score = scoreWithNavigation.Score,
            AcademicYear = scoreWithNavigation.AcademicYear,
            Term = scoreWithNavigation.Term,
            RecordedAt = scoreWithNavigation.RecordedAt,
            RecordedByName = scoreWithNavigation.RecordedByTeacher?.FullName,
            // Include comment fields
            Comments = scoreWithNavigation.Comments,
            CommentsUpdatedAt = scoreWithNavigation.CommentsUpdatedAt,
            CommentsUpdatedByName = scoreWithNavigation.CommentsUpdatedByTeacher?.FullName
        };

        _logger.LogInformation("Returning score response: {@ResponseDto}", responseDto);

        return Ok(responseDto);
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogError(ex, "Unauthorized access in CreateOrUpdateScore");
        return Unauthorized("Access denied");
    }
    catch (ArgumentException ex)
    {
        _logger.LogError(ex, "Invalid argument in CreateOrUpdateScore");
        return BadRequest(ex.Message);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error in CreateOrUpdateScore");
        return StatusCode(500, "An unexpected error occurred while processing the request");
    }
}
        /// <summary>
        /// Get all exam types
        /// </summary>
        [HttpGet("types")]
        [Authorize(Roles = "Admin,Teacher,Staff")]
        //[Authorize]
        public async Task<ActionResult<IEnumerable<ExamType>>> GetExamTypes()
        {
            try
            {
                var examTypes = await _examService.GetExamTypesAsync();
                return Ok(examTypes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving exam types");
                return StatusCode(500, "An error occurred while retrieving exam types");
            }
        }

        /// <summary>
        /// Create new exam type (Admin only)
        /// </summary>
        [HttpPost("types")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExamType>> CreateExamType([FromBody] CreateExamTypeDto createExamTypeDto)
        {
            try
            {
                if (createExamTypeDto == null)
                    return BadRequest("Exam type data is required");

                var examType = new ExamType
                {
                    Name = createExamTypeDto.Name,
                    Description = createExamTypeDto.Description,
                    Order = createExamTypeDto.Order
                    //Order = await _examService.GetNextOrderValueAsync()
                };

                var createdExamType = await _examService.CreateExamTypeAsync(examType);
                return CreatedAtAction(nameof(GetExamTypes), new { id = createdExamType.Id }, createdExamType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating exam type");
                return StatusCode(500, "An error occurred while creating the exam type");
            }
        }

        /// <summary>
        /// Update exam type - NEW ENDPOINT
        /// </summary>
        [HttpPut("types/{examTypeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ExamType>> UpdateExamType(int examTypeId, [FromBody] UpdateExamTypeDto updateExamTypeDto)
        {
            try
            {
                var examType = await _context.ExamTypes.FindAsync(examTypeId);
                if (examType == null)
                {
                    return NotFound("Exam type not found");
                }

                examType.Name = updateExamTypeDto.Name;
                examType.Description = updateExamTypeDto.Description;
                examType.Order = updateExamTypeDto.Order;

                await _context.SaveChangesAsync();
                return Ok(examType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exam type {ExamTypeId}", examTypeId);
                return StatusCode(500, "An error occurred while updating the exam type");
            }
        }

        /// <summary>
        /// Delete exam type - NEW ENDPOINT
        /// </summary>
        [HttpDelete("types/{examTypeId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteExamType(int examTypeId)
        {
            try
            {
                var examType = await _context.ExamTypes.FindAsync(examTypeId);
                if (examType == null)
                {
                    return NotFound("Exam type not found");
                }

                // Check if exam type is in use
                var isInUse = await _context.ExamScores.AnyAsync(s => s.ExamTypeId == examTypeId);
                if (isInUse)
                {
                    return BadRequest("Cannot delete exam type that is currently in use");
                }

                _context.ExamTypes.Remove(examType);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Exam type deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting exam type {ExamTypeId}", examTypeId);
                return StatusCode(500, "An error occurred while deleting the exam type");
            }
        }

        /// <summary>
        /// Get teacher's assigned subjects and grades
        /// </summary>
        [HttpGet("teacher/assignments")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<TeacherAssignmentDto>>> GetTeacherAssignments()
        {
            try
            {
                var teacherIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(teacherIdClaim) || !int.TryParse(teacherIdClaim, out int teacherId))
                {
                    return Unauthorized("Invalid teacher credentials");
                }

                var assignments = await _context.TeacherSubjectAssignments
                    .Include(tsa => tsa.Subject)
                    .Include(tsa => tsa.Grade)
                    .Where(tsa => tsa.TeacherId == teacherId && tsa.IsActive)
                    .ToListAsync();

                var assignmentDtos = assignments.Select(a => new TeacherAssignmentDto
                {
                    Id = a.Id,
                    SubjectId = a.SubjectId,
                    SubjectName = a.Subject?.Name,
                    GradeId = a.GradeId,
                    GradeName = a.Grade?.FullName,
                    GradeSection = a.Grade != null ? a.Grade.Section : (SchoolSection?)null,
                    AssignedAt = a.AssignedAt
                });

                return Ok(assignmentDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving teacher assignments");
                return StatusCode(500, "An error occurred while retrieving teacher assignments");
            }
        }

        // New admin endpoint to monitor all teacher assignments
[HttpGet("admin/teacher-assignments")]
[Authorize(Roles = "Admin,SuperAdmin")]
public async Task<ActionResult<IEnumerable<TeacherAssignmentDto>>> GetAllTeacherAssignments(
    [FromQuery] int? teacherId = null,
    [FromQuery] int? subjectId = null,
    [FromQuery] int? gradeId = null,
    [FromQuery] bool includeInactive = false)
{
    try
    {
        var query = _context.TeacherSubjectAssignments
            .Include(tsa => tsa.Subject)
            .Include(tsa => tsa.Grade)
            .Include(tsa => tsa.Teacher)
            .AsQueryable();

        // Apply filters
        if (teacherId.HasValue)
            query = query.Where(tsa => tsa.TeacherId == teacherId.Value);

        if (subjectId.HasValue)
            query = query.Where(tsa => tsa.SubjectId == subjectId.Value);

        if (gradeId.HasValue)
            query = query.Where(tsa => tsa.GradeId == gradeId.Value);

        if (!includeInactive)
            query = query.Where(tsa => tsa.IsActive);

        var assignments = await query
            .OrderBy(tsa => tsa.Teacher.FullName)
            .ThenBy(tsa => tsa.Subject.Name)
            .ToListAsync();

        var assignmentDtos = assignments.Select(a => new TeacherAssignmentDto
        {
            Id = a.Id,
            TeacherId = a.TeacherId,
            TeacherName = $"{a.Teacher.FullName}",
            SubjectId = a.SubjectId,
            SubjectName = a.Subject?.Name,
            GradeId = a.GradeId,
            GradeName = a.Grade?.FullName,
            GradeSection = a.Grade != null ? a.Grade.Section : (SchoolSection?)null,
            AssignedAt = a.AssignedAt,
            IsActive = a.IsActive
        });

        return Ok(assignmentDtos);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving all teacher assignments for admin");
        return StatusCode(500, "An error occurred while retrieving teacher assignments");
    }
}

// Additional admin endpoint to get assignment statistics
[HttpGet("admin/teacher-assignments/stats")]
[Authorize(Roles = "Admin,Staff")]
public async Task<ActionResult<object>> GetTeacherAssignmentStats()
{
    try
    {
        // Get raw data first, then group in memory
        var assignments = await _context.TeacherSubjectAssignments
            .Include(tsa => tsa.Subject)
            .Include(tsa => tsa.Grade)
            .Where(tsa => tsa.IsActive)
            .Select(tsa => new
            {
                SubjectName = tsa.Subject.Name,
                GradeName = tsa.Grade.Name, // Use Name instead of FullName if FullName isn't mapped
                TeacherId = tsa.TeacherId
            })
            .ToListAsync();

        // Group and aggregate in memory
        var stats = assignments
            .GroupBy(a => new { a.SubjectName, a.GradeName })
            .Select(g => new
            {
                Subject = g.Key.SubjectName,
                Grade = g.Key.GradeName,
                TeacherCount = g.Count()
            })
            .OrderBy(s => s.Grade)
            .ThenBy(s => s.Subject)
            .ToList();

        var totalActiveAssignments = await _context.TeacherSubjectAssignments
            .CountAsync(tsa => tsa.IsActive);

        var totalTeachersWithAssignments = await _context.TeacherSubjectAssignments
            .Where(tsa => tsa.IsActive)
            .Select(tsa => tsa.TeacherId)
            .Distinct()
            .CountAsync();

        return Ok(new
        {
            TotalActiveAssignments = totalActiveAssignments,
            TotalTeachersWithAssignments = totalTeachersWithAssignments,
            AssignmentsBySubjectAndGrade = stats
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error retrieving teacher assignment statistics");
        return StatusCode(500, "An error occurred while retrieving assignment statistics");
    }
}
        /// <summary>
        /// Debug endpoint to check authentication status - NO ROLE RESTRICTION for debugging
        /// </summary>
        [HttpGet("debug/auth")]
        [Authorize] // Remove role restriction temporarily for debugging
        public ActionResult<object> DebugAuth()
        {
            return Ok(new
            {
                IsAuthenticated = User?.Identity?.IsAuthenticated,
                Identity = User?.Identity?.Name,
                Claims = User?.Claims?.Select(c => new { c.Type, c.Value }).ToList(),
                TeacherId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                // Check different role claim types
                RoleClaimStandard = User?.FindFirst(ClaimTypes.Role)?.Value,
                RoleClaimDirect = User?.FindFirst("role")?.Value,
                RoleClaimHttp = User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value,
                AllRoleClaims = User?.Claims?.Where(c => c.Type.Contains("role", StringComparison.OrdinalIgnoreCase))
                    .Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        /// <summary>
        /// Alternative debug endpoint that checks roles manually
        /// </summary>
        [HttpGet("debug/roles")]
        [Authorize]
        public ActionResult<object> DebugRoles()
        {
            var hasTeacherRole = User?.IsInRole("Teacher");
            var hasTeacherRoleManual = User?.Claims?.Any(c => 
                (c.Type == ClaimTypes.Role || c.Type == "role") && 
                c.Value.Equals("Teacher", StringComparison.OrdinalIgnoreCase));

            return Ok(new
            {
                HasTeacherRoleBuiltIn = hasTeacherRole,
                HasTeacherRoleManual = hasTeacherRoleManual,
                AllClaims = User?.Claims?.Select(c => new { c.Type, c.Value }).ToList()
            });
        }

        // Helper method for grade calculation
        private string CalculateGrade(decimal score)
        {
            if (score >= 90) return "A";
            if (score >= 80) return "B";
            if (score >= 70) return "C";
            if (score >= 60) return "D";
            return "F";
        }
    }
}