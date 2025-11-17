// ===== DATABASE INITIALIZATION SERVICE =====

using BluebirdCore.Data;
using Microsoft.EntityFrameworkCore;

namespace BluebirdCore.Services
{
    public interface IDatabaseInitializer
    {
        Task InitializeAsync();
    }

    public class DatabaseInitializer : IDatabaseInitializer
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(SchoolDbContext context, ILogger<DatabaseInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                        // Check if database exists
        var canConnect = await _context.Database.CanConnectAsync();
        
        if (!canConnect)
        {
            _logger.LogInformation("Database does not exist. Creating database...");
            await _context.Database.EnsureCreatedAsync();
        }
        else
        {
            _logger.LogInformation("Database exists. Skipping migrations to avoid conflicts with existing schema.");
            // Skip migrations for now since database already has schema
        }

                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database initialization");
                
                // If there's a foreign key constraint issue, log specific help
                if (ex.Message.Contains("FK_Grades_Users_HomeroomTeacherId") || ex.Message.Contains("SET NULL"))
                {
                    _logger.LogError("Foreign key constraint issue detected. Please run the following commands:");
                    _logger.LogError("1. Remove-Migration (if you have existing migrations)");
                    _logger.LogError("2. Add-Migration InitialCreate");
                    _logger.LogError("3. Update-Database");
                    _logger.LogError("Or alternatively: Drop-Database and then Update-Database");
                }
                
                throw;
            }
        }
    }
}