using SchoolErpSMS.Entities;

namespace SchoolErpSMS.Services
{
    public interface IBabyClassSkillService
    {
        Task<IEnumerable<BabyClassSkill>> GetAllSkillsAsync();
        Task<IEnumerable<BabyClassSkillItem>> GetSkillItemsBySkillIdAsync(int skillId);
        Task<IEnumerable<BabyClassSkillAssessment>> GetStudentAssessmentsAsync(int studentId, int academicYear, int term);
        Task<BabyClassSkillAssessment?> GetAssessmentAsync(int studentId, int skillItemId, int academicYear, int term);
        Task<BabyClassSkillAssessment?> GetAssessmentByIdAsync(int assessmentId);
        Task<BabyClassSkillAssessment> CreateOrUpdateAssessmentAsync(int studentId, int skillItemId, int academicYear, int term, string? teacherComment, int assessedBy);
        Task<bool> DeleteAssessmentAsync(int assessmentId);
        Task<IEnumerable<BabyClassSkillAssessment>> GetClassAssessmentsAsync(int gradeId, int academicYear, int term);
    }
}
