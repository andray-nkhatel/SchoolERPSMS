using SchoolErpSMS.Data;
using SchoolErpSMS.Entities;
using Microsoft.EntityFrameworkCore;
using SchoolErpSMS.DTOs;

namespace SchoolErpSMS.Services
{
     public interface IUserService
    {
        Task<User?> AuthenticateAsync(string username, string password);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User> CreateUserAsync(User user, string password);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> ResetPasswordAsync(int userId, string newPassword);
        Task<bool> DeactivateUserAsync(int userId);
        Task<IEnumerable<TeacherAssignmentDtoMinimal>> GetTeacherAssignmentsAsync(int teacherId, bool includeInactive = false);
    }

    public class UserService : IUserService
    {
        private readonly SchoolDbContext _context;

        public UserService(SchoolDbContext context)
        {
            _context = context;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            user.LastLoginAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> CreateUserAsync(User user, string password)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            user.CreatedAt = DateTime.Now;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeactivateUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<TeacherAssignmentDtoMinimal>> GetTeacherAssignmentsAsync(int teacherId, bool includeInactive = false)
        {
            // Use your ORM/EF to fetch all assignments for this teacher in one query
            var query = _context.TeacherSubjectAssignments
                .Where(a => a.TeacherId == teacherId);

            if (!includeInactive)
            {
                query = query.Where(a => a.IsActive);
            }

            return await query
                .Select(a => new TeacherAssignmentDtoMinimal
                {
                    AssignmentId = a.Id,
                    SubjectId = a.SubjectId,
                    GradeId = a.GradeId,
                    TeacherId = a.TeacherId
                })
                .ToListAsync();
        }
    }
}