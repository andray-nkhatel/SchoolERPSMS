using SchoolErpSMS.Data;
using SchoolErpSMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace SchoolErpSMS.Services
{
    public interface IExamService
    {
        Task<IEnumerable<ExamScore>> GetScoresByStudentAsync(int studentId, int academicYear, int term);
        Task<IEnumerable<ExamScore>> GetScoresByGradeAsync(int gradeId, int academicYear, int term);
        Task<ExamScore> CreateOrUpdateScoreAsync(ExamScore score);
        Task<bool> CanTeacherEnterScore(int teacherId, int subjectId, int gradeId);
        Task<bool> CanTeacherEnterScoreForStudent(int teacherId, int subjectId, int gradeId, int studentId);
        Task<IEnumerable<ExamType>> GetExamTypesAsync();
        
        // Absent helper methods
        bool IsStudentAbsent(ExamScore score);
        void MarkAsAbsent(ExamScore score, bool isAbsent);
        Task<ExamType> CreateExamTypeAsync(ExamType examType);
    }

    public class ExamService : IExamService
    {
        private readonly SchoolDbContext _context;

        public ExamService(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ExamScore>> GetScoresByStudentAsync(int studentId, int academicYear, int term)
        {
            return await _context.ExamScores
                .Include(es => es.Subject)
                .Include(es => es.ExamType)
                .Where(es => es.StudentId == studentId && es.AcademicYear == academicYear && es.Term == term)
                .OrderBy(es => es.Subject.Name)
                .ThenBy(es => es.ExamType.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<ExamScore>> GetScoresByGradeAsync(int gradeId, int academicYear, int term)
        {
            return await _context.ExamScores
                .Include(es => es.Student)
                .Include(es => es.Subject)
                .Include(es => es.ExamType)
                .Where(es => es.GradeId == gradeId && es.AcademicYear == academicYear && es.Term == term)
                .OrderBy(es => es.Student.LastName)
                .ThenBy(es => es.Student.FirstName)
                .ThenBy(es => es.Subject.Name)
                .ToListAsync();
        }

        public async Task<ExamScore> CreateOrUpdateScoreAsync(ExamScore score)
        {
            var existingScore = await _context.ExamScores
                .FirstOrDefaultAsync(es => es.StudentId == score.StudentId 
                                         && es.SubjectId == score.SubjectId 
                                         && es.ExamTypeId == score.ExamTypeId 
                                         && es.AcademicYear == score.AcademicYear 
                                         && es.Term == score.Term);
        
            if (existingScore != null)
            {
                // Update existing score
                existingScore.Score = score.Score;
                existingScore.IsAbsent = score.IsAbsent;
                existingScore.RecordedAt = DateTime.UtcNow;
                existingScore.RecordedBy = score.RecordedBy;
                
                // Handle comments - only update timestamp if comments actually changed
                if (existingScore.Comments != score.Comments)
                {
                    existingScore.Comments = score.Comments;
                    existingScore.CommentsUpdatedAt = DateTime.UtcNow;
                    existingScore.CommentsUpdatedBy = score.RecordedBy;
                }
                // If comments are the same, preserve existing comment metadata
            }
            else
            {
                // Create new score
                score.RecordedAt = DateTime.UtcNow;
                
                // Set comment metadata for new scores if comments exist
                if (!string.IsNullOrWhiteSpace(score.Comments))
                {
                    score.CommentsUpdatedAt = DateTime.UtcNow;
                    score.CommentsUpdatedBy = score.RecordedBy;
                }
                
                _context.ExamScores.Add(score);
            }
        
            await _context.SaveChangesAsync();
            return existingScore ?? score;
        }

        public async Task<bool> CanTeacherEnterScore(int teacherId, int subjectId, int gradeId)
        {
            return await _context.TeacherSubjectAssignments
                .AnyAsync(tsa => tsa.TeacherId == teacherId 
                              && tsa.SubjectId == subjectId 
                              && tsa.GradeId == gradeId 
                              && tsa.IsActive);
        }

        public async Task<bool> CanTeacherEnterScoreForStudent(int teacherId, int subjectId, int gradeId, int studentId)
        {
            // First check if teacher is assigned to teach this subject in this grade
            var teacherAssigned = await _context.TeacherSubjectAssignments
                .AnyAsync(tsa => tsa.TeacherId == teacherId 
                              && tsa.SubjectId == subjectId 
                              && tsa.GradeId == gradeId 
                              && tsa.IsActive);

            if (!teacherAssigned) return false;

            // Check if this is an optional subject
            var isOptionalSubject = await _context.GradeSubjects
                .AnyAsync(gs => gs.SubjectId == subjectId 
                             && gs.GradeId == gradeId 
                             && gs.IsOptional 
                             && gs.IsActive);

            // If it's an optional subject, check if student is enrolled
            if (isOptionalSubject)
            {
                return await _context.StudentOptionalSubjects
                    .AnyAsync(sos => sos.StudentId == studentId && sos.SubjectId == subjectId);
            }

            // For core subjects, teacher can enter scores for all students in the grade
            return true;
        }

        public async Task<IEnumerable<ExamType>> GetExamTypesAsync()
        {
            return await _context.ExamTypes
                .Where(et => et.IsActive)
                .OrderBy(et => et.Order)
                .ToListAsync();
        }

        public async Task<ExamType> CreateExamTypeAsync(ExamType examType)
        {
            _context.ExamTypes.Add(examType);
            await _context.SaveChangesAsync();
            return examType;
        }

        // Absent helper methods using IsAbsent field
        public bool IsStudentAbsent(ExamScore score)
        {
            return score.IsAbsent;
        }

        public void MarkAsAbsent(ExamScore score, bool isAbsent)
        {
            score.IsAbsent = isAbsent;
            
            // If marking as absent, set score to 0
            if (isAbsent)
            {
                score.Score = 0;
            }
        }
    }
}