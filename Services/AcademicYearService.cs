using BluebirdCore.Data;
using BluebirdCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BluebirdCore.Services
{
    public interface IAcademicYearService
    {
        Task<IEnumerable<AcademicYear>> GetAllAcademicYearsAsync();
        Task<AcademicYear> GetActiveAcademicYearAsync();
        Task<AcademicYear> CreateAcademicYearAsync(AcademicYear academicYear);
        Task<bool> CloseAcademicYearAsync(int academicYearId);
        Task<AcademicYear?> GetAcademicYearByIdAsync(int academicYearId);
        Task<bool> DeleteAcademicYearAsync(int academicYearId);
       
    }

    public class AcademicYearService : IAcademicYearService
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<AcademicYearService> _logger;

        public AcademicYearService(
            SchoolDbContext context,
            ILogger<AcademicYearService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<AcademicYear>> GetAllAcademicYearsAsync()
        {
            return await _context.AcademicYears
                .OrderByDescending(ay => ay.StartDate)
                .ToListAsync();
        }

        public async Task<AcademicYear> GetActiveAcademicYearAsync()
        {
            return await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.IsActive);
        }

        public async Task<AcademicYear?> GetAcademicYearByIdAsync(int academicYearId)
        {
            return await _context.AcademicYears
                .FirstOrDefaultAsync(ay => ay.Id == academicYearId);
        }

        public async Task<AcademicYear> CreateAcademicYearAsync(AcademicYear academicYear)
        {
            // Deactivate current active academic year
            var currentActive = await GetActiveAcademicYearAsync();
            if (currentActive != null)
            {
                currentActive.IsActive = false;
            }

            academicYear.IsActive = true;
            _context.AcademicYears.Add(academicYear);
            await _context.SaveChangesAsync();

            return academicYear;
        }

        public async Task<bool> CloseAcademicYearAsync(int academicYearId)
        {
            var academicYear = await _context.AcademicYears.FindAsync(academicYearId);
            if (academicYear == null) return false;

            academicYear.IsClosed = true;
            academicYear.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }


        


        public async Task<bool> DeleteAcademicYearAsync(int academicYearId)
        {
            var academicYear = await _context.AcademicYears.FindAsync(academicYearId);
            if (academicYear == null)
                return false;

            // Optionally: Check for related data before deleting
            _context.AcademicYears.Remove(academicYear);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SetActiveAcademicYearAsync(int academicYearId)
        {
            var academicYear = await _context.AcademicYears.FindAsync(academicYearId);
            if (academicYear == null)
                return false;

            // Deactivate all other academic years
            var allYears = await _context.AcademicYears.ToListAsync();
            foreach (var ay in allYears)
            {
                ay.IsActive = ay.Id == academicYearId;
            }
            await _context.SaveChangesAsync();
            return true;
        }

    }
}