// ===== DATABASE INITIALIZATION SERVICE =====

using SchoolErpSMS.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace SchoolErpSMS.Services
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
                
                // Apply pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation($"Applying {pendingMigrations.Count()} pending migration(s)...");
                    try
                    {
                        await _context.Database.MigrateAsync();
                        _logger.LogInformation("Migrations applied successfully.");
                    }
                    catch (Exception sqlEx) when (sqlEx.Message.Contains("2714") || sqlEx.Message.Contains("already an object named") || sqlEx.InnerException?.Message?.Contains("2714") == true)
                    {
                        _logger.LogWarning("Tables already exist in database. Marking migration(s) as applied...");
                        // Mark all pending migrations as applied since tables already exist
                        foreach (var migrationId in pendingMigrations)
                        {
                            try
                            {
                                await _context.Database.ExecuteSqlRawAsync(
                                    $@"IF NOT EXISTS (SELECT 1 FROM __EFMigrationsHistory WHERE MigrationId = '{migrationId}') 
                                       INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) 
                                       VALUES ('{migrationId}', '9.0.5')");
                                _logger.LogInformation($"Migration '{migrationId}' marked as applied.");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning($"Could not mark migration '{migrationId}' as applied: {ex.Message}");
                            }
                        }
                        _logger.LogInformation("All migrations marked as applied.");
                    }
                }
                else
                {
                    _logger.LogInformation("Database is up to date. No pending migrations.");
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