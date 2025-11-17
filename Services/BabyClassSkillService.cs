using BluebirdCore.Data;
using BluebirdCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BluebirdCore.Services
{
    public class BabyClassSkillService : IBabyClassSkillService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<BabyClassSkillService> _logger;

        public BabyClassSkillService(SchoolDbContext context, ILogger<BabyClassSkillService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<BabyClassSkill>> GetAllSkillsAsync()
        {
            return await _context.BabyClassSkills
                .Where(s => s.IsActive)
                .OrderBy(s => s.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<BabyClassSkillItem>> GetSkillItemsBySkillIdAsync(int skillId)
        {
            return await _context.BabyClassSkillItems
                .Where(si => si.SkillId == skillId && si.IsActive)
                .OrderBy(si => si.Order)
                .ToListAsync();
        }

        public async Task<IEnumerable<BabyClassSkillAssessment>> GetStudentAssessmentsAsync(int studentId, int academicYear, int term)
        {
            try
            {
                _logger.LogInformation("Querying assessments for StudentId: {StudentId}, AcademicYear: {AcademicYear}, Term: {Term}", 
                    studentId, academicYear, term);

                var query = _context.BabyClassSkillAssessments
                    .Include(a => a.Student)
                    .Include(a => a.SkillItem)
                        .ThenInclude(si => si.Skill)
                    .Where(a => a.StudentId == studentId && a.AcademicYear == academicYear && a.Term == term)
                    .OrderBy(a => a.SkillItem.Skill.Order)
                    .ThenBy(a => a.SkillItem.Order);

                _logger.LogInformation("Executing query: {Query}", query.ToQueryString());
                
                var result = await query.ToListAsync();
                
                _logger.LogInformation("Query executed successfully, returned {Count} assessments", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database error in GetStudentAssessmentsAsync for StudentId: {StudentId}, AcademicYear: {AcademicYear}, Term: {Term}. " +
                    "Exception: {ExceptionType} - {Message}", studentId, academicYear, term, ex.GetType().Name, ex.Message);
                throw;
            }
        }

        public async Task<BabyClassSkillAssessment?> GetAssessmentAsync(int studentId, int skillItemId, int academicYear, int term)
        {
            return await _context.BabyClassSkillAssessments
                .Include(a => a.Student)
                .Include(a => a.SkillItem)
                    .ThenInclude(si => si.Skill)
                
                .FirstOrDefaultAsync(a => a.StudentId == studentId && 
                                        a.SkillItemId == skillItemId && 
                                        a.AcademicYear == academicYear && 
                                        a.Term == term);
        }

        public async Task<BabyClassSkillAssessment> CreateOrUpdateAssessmentAsync(int studentId, int skillItemId, int academicYear, int term, string? teacherComment, int assessedBy)
        {
            var existingAssessment = await GetAssessmentAsync(studentId, skillItemId, academicYear, term);
            
            if (existingAssessment != null)
            {
                // Update existing assessment
                existingAssessment.TeacherComment = teacherComment;
                existingAssessment.AssessedAt = DateTime.Now;
                existingAssessment.AssessedBy = assessedBy;
                
                _context.BabyClassSkillAssessments.Update(existingAssessment);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Updated skill assessment for Student {StudentId}, SkillItem {SkillItemId}", studentId, skillItemId);
                return existingAssessment;
            }
            else
            {
                // Create new assessment
                var newAssessment = new BabyClassSkillAssessment
                {
                    StudentId = studentId,
                    SkillItemId = skillItemId,
                    AcademicYear = academicYear,
                    Term = term,
                    TeacherComment = teacherComment,
                    AssessedBy = assessedBy,
                    AssessedAt = DateTime.Now
                };
                
                _context.BabyClassSkillAssessments.Add(newAssessment);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new skill assessment for Student {StudentId}, SkillItem {SkillItemId}", studentId, skillItemId);
                return newAssessment;
            }
        }

        public async Task<bool> DeleteAssessmentAsync(int assessmentId)
        {
            var assessment = await _context.BabyClassSkillAssessments.FindAsync(assessmentId);
            if (assessment == null)
                return false;
            
            _context.BabyClassSkillAssessments.Remove(assessment);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Deleted skill assessment {AssessmentId}", assessmentId);
            return true;
        }

        public async Task<BabyClassSkillAssessment?> GetAssessmentByIdAsync(int assessmentId)
        {
            return await _context.BabyClassSkillAssessments
                .Include(a => a.Student)
                .Include(a => a.SkillItem)
                    .ThenInclude(si => si.Skill)
                
                .FirstOrDefaultAsync(a => a.Id == assessmentId);
        }

        public async Task<IEnumerable<BabyClassSkillAssessment>> GetClassAssessmentsAsync(int gradeId, int academicYear, int term)
        {
            return await _context.BabyClassSkillAssessments
                .Include(a => a.Student)
                    .ThenInclude(s => s.Grade)
                .Include(a => a.SkillItem)
                    .ThenInclude(si => si.Skill)
                
                .Where(a => a.Student.GradeId == gradeId && a.AcademicYear == academicYear && a.Term == term)
                .OrderBy(a => a.Student.LastName)
                .ThenBy(a => a.Student.FirstName)
                .ThenBy(a => a.SkillItem.Skill.Order)
                .ThenBy(a => a.SkillItem.Order)
                .ToListAsync();
        }
    }
}
