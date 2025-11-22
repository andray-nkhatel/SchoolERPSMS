using BluebirdCore.Data;
using BluebirdCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BluebirdCore.Services
{
    public interface IDatabaseSeeder
    {
        Task SeedAsync();
    }

    public class DatabaseSeeder : IDatabaseSeeder
    {
        private readonly SchoolDbContext _context;
        private readonly ILogger<DatabaseSeeder> _logger;

        public DatabaseSeeder(SchoolDbContext context, ILogger<DatabaseSeeder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            try
            {
                //_logger.LogInformation("🌱 Starting database seeding...");

                // Check if database is accessible
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    //_logger.LogError("❌ Cannot connect to database");
                    return;
                }

                //_logger.LogInformation("✅ Database connection successful");

                // Count existing users
                var userCount = await _context.Users.CountAsync();
                //_logger.LogInformation($"📊 Current user count: {userCount}");

                // List existing users
                var existingUsers = await _context.Users.Select(u => new { u.Username, u.Email, u.Role, u.IsActive }).ToListAsync();
                foreach (var user in existingUsers)
                {
                    //_logger.LogInformation($"👤 Existing user: {user.Username} ({user.Email}) [{user.Role}] - Active: {user.IsActive}");
                }

                // Check for teacher by username OR email to avoid duplicates
                var teacherExists = await _context.Users.AnyAsync(u => 
                    u.Username == "mutale" || u.Email == "mutale@lubwe.sch.edu");
                //_logger.LogInformation($"🔍 Teacher 'mwiindec' or email 'clement.mwiinde@chs.edu' exists: {teacherExists}");

                if (!teacherExists)
                {
                    //_logger.LogInformation("➕ Creating teacher user...");
                    
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword("mutale123");
                   // _logger.LogInformation($"🔐 Generated password hash: {passwordHash.Substring(0, 20)}...");

                    var teacherUser = new User
                    {
                        Username = "mutale",
                        PasswordHash = passwordHash,
                        FullName = "Mr. Mutale",
                        Email = "mutale@lubwe.sch.edu",
                        Role = UserRole.Teacher,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Users.Add(teacherUser);
                    
                    try
                    {
                        await _context.SaveChangesAsync();
                        //_logger.LogInformation("✅ Teacher created successfully");
                        
                        // Verify the user was created
                        var createdUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "mutale");
                        if (createdUser != null)
                        {
                           // _logger.LogInformation($"✅ Verification: Teacher found with ID {createdUser.Id}");
                            
                            // Test password verification
                            var passwordTest = BCrypt.Net.BCrypt.Verify("mutale123", createdUser.PasswordHash);
                            // _logger.LogInformation($"🔐 Password verification test: {passwordTest}");
                        }
                        else
                        {
                            //_logger.LogError("❌ Teacher was not found after creation");
                        }
                    }
                    catch (Exception saveEx)
                    {
                       // _logger.LogError(saveEx, "❌ Failed to save teacher user");
                        throw;
                    }
                }
                else
                {
                    //_logger.LogInformation("ℹ️ Teacher already exists, skipping creation");
                    
                    // Find existing teacher by username or email
                    var existingTeacher = await _context.Users.FirstOrDefaultAsync(u => 
                        u.Username == "mutale" || u.Email == "mutale@lubwe.sch.edu");
                    
                    if (existingTeacher != null)
                    {
//_logger.LogInformation($"📋 Existing teacher details:");
//_logger.LogInformation($"   - ID: {existingTeacher.Id}");
//_logger.LogInformation($"   - Username: {existingTeacher.Username}");
//_logger.LogInformation($"   - Email: {existingTeacher.Email}");
//_logger.LogInformation($"   - FullName: {existingTeacher.FullName}");
//_logger.LogInformation($"   - Role: {existingTeacher.Role}");
//_logger.LogInformation($"   - IsActive: {existingTeacher.IsActive}");
//_logger.LogInformation($"   - CreatedAt: {existingTeacher.CreatedAt}");
                        
                        // Update user details if needed (in case found by email but username is different)
                        bool needsUpdate = false;
                        
                        if (existingTeacher.Username != "mutale")
                        {
                            //_logger.LogInformation($"🔄 Updating username from '{existingTeacher.Username}' to 'mwiindec'");
                            existingTeacher.Username = "mutale";
                            needsUpdate = true;
                        }
                        
                        if (existingTeacher.Email != "mutale@lubwe.sch.edu")
                        {
                            //_logger.LogInformation($"🔄 Updating email from '{existingTeacher.Email}' to 'clement.mwiinde@chs.edu'");
                            existingTeacher.Email = "mutale@lubwe.sch.edu";
                            needsUpdate = true;
                        }
                        
                        if (existingTeacher.FullName != "Mr. Mutale")
                        {
                            //_logger.LogInformation($"🔄 Updating full name from '{existingTeacher.FullName}' to 'Clement Mwiinde'");
                            existingTeacher.FullName = "Mr. Mutale";
                            needsUpdate = true;
                        }
                        
                        if (existingTeacher.Role != UserRole.Teacher)
                        {
                            //_logger.LogInformation($"🔄 Updating role from '{existingTeacher.Role}' to 'Teacher'");
                            existingTeacher.Role = UserRole.Teacher;
                            needsUpdate = true;
                        }
                        
                        // Test password verification on existing user
                        var passwordTest = BCrypt.Net.BCrypt.Verify("mutale23", existingTeacher.PasswordHash);
                        //_logger.LogInformation($"🔐 Existing user password verification: {passwordTest}");
                        
                        if (!passwordTest)
                        {
                            //_logger.LogWarning("⚠️ Password verification failed for existing teacher - updating password");
                            existingTeacher.PasswordHash = BCrypt.Net.BCrypt.HashPassword("mutale123");
                            needsUpdate = true;
                        }
                        
                        if (needsUpdate)
                        {
                            try
                            {
                                await _context.SaveChangesAsync();
                                //_logger.LogInformation("✅ Teacher details updated successfully");
                            }
                            catch (Exception updateEx)
                            {
                                //_logger.LogError(updateEx, "❌ Failed to update existing teacher");
                                // Don't throw - this is not critical
                            }
                        }
                    }
                }

                // Add sample student if not exists
                var studentCount = await _context.Students.CountAsync();
                _logger.LogInformation($"📊 Current student count: {studentCount}");

                Student? johnMbuki = null;
                if (studentCount == 0)
                {
                    //_logger.LogInformation("➕ Creating sample student...");
                    
                    var sampleStudent = new Student
                    {
                        FirstName = "John",
                        LastName = "Mbuki",
                        StudentNumber = "24001",
                        DateOfBirth = new DateTime(2010, 5, 15),
                        Gender = "Male",
                        GradeId = 32, // Grade 1
                        GuardianName = "John Doe",
                        GuardianPhone = "+260123456789",
                        Address = "123 Sample Street, Lusaka",
                        EnrollmentDate = DateTime.UtcNow
                    };

                    _context.Students.Add(sampleStudent);
                    
                    try
                    {
                        await _context.SaveChangesAsync();
                        johnMbuki = sampleStudent;
                        //_logger.LogInformation("✅ Sample student created successfully");
                    }
                    catch (Exception studentEx)
                    {
                        //  _logger.LogError(
                        // Don't throw - student creation is not critical
                    }
                }
                else
                {
                    //_logger.LogInformation("ℹ️ Students already exist, skipping student creation");
                    // Find John Mbuki if he exists
                    johnMbuki = await _context.Students
                        .FirstOrDefaultAsync(s => s.FirstName == "John" && s.LastName == "Mbuki");
                }

                // Seed exam scores for John Mbuki if he exists
                if (johnMbuki != null)
                {
                    await SeedStudentExamScoresAsync(johnMbuki);
                }

                // Final verification
                var finalUserCount = await _context.Users.CountAsync();
                var finalStudentCount = await _context.Students.CountAsync();
                
                //_logger.LogInformation($"🏁 Database seeding completed successfully");
                //_logger.LogInformation($"📊 Final counts - Users: {finalUserCount}, Students: {finalStudentCount}");

                // List all users for verification
                var allUsers = await _context.Users.Select(u => new { u.Username, u.Email, u.Role, u.IsActive }).ToListAsync();
                //_logger.LogInformation("👥 All users in system:");
                foreach (var user in allUsers)
                {
                    //_logger.LogInformation($"   - {user.Username} ({user.Email}) [{user.Role}] - Active: {user.IsActive}");
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "❌ Error occurred during database seeding");
                throw;
            }
        }

                private async Task SeedBabyClassSkillsAsync()
        {
            try
            {
                // Check if Baby Class Skills already exist
                var existingSkills = await _context.BabyClassSkills.CountAsync();
                if (existingSkills > 0)
                {
                    //_logger.LogInformation("ℹ️ Baby Class Skills already exist, skipping seeding");
                    return;
                }

                //_logger.LogInformation("🌱 Seeding Baby Class Skills...");

                // Create Baby Class Skills
                var skills = new List<BabyClassSkill>
                {
                    new BabyClassSkill { Id = 1, Name = "Communication Skills", Description = "Verbal communication abilities", Order = 1, IsActive = true },
                    new BabyClassSkill { Id = 2, Name = "Social Emotional Skills", Description = "Social interaction and emotional development", Order = 2, IsActive = true },
                    new BabyClassSkill { Id = 3, Name = "Reading & Writing", Description = "Early literacy skills", Order = 3, IsActive = true },
                    new BabyClassSkill { Id = 4, Name = "Colour & Shapes", Description = "Visual recognition and identification", Order = 4, IsActive = true },
                    new BabyClassSkill { Id = 5, Name = "Numbers", Description = "Basic numeracy skills", Order = 5, IsActive = true },
                    new BabyClassSkill { Id = 6, Name = "Fine-Motor Skills", Description = "Small muscle coordination and control", Order = 6, IsActive = true },
                    new BabyClassSkill { Id = 7, Name = "Gross Motor Skills", Description = "Large muscle movement and coordination", Order = 7, IsActive = true }
                };

                _context.BabyClassSkills.AddRange(skills);

                // Create Baby Class Skill Items
                var skillItems = new List<BabyClassSkillItem>
                {
                    // Communication Skills (SkillId = 1)
                    new BabyClassSkillItem { Id = 1, SkillId = 1, Name = "Speaks Clearly", Description = "Ability to articulate words clearly", Order = 1, IsActive = true },
                    new BabyClassSkillItem { Id = 2, SkillId = 1, Name = "Responds to direct questions", Description = "Ability to answer questions appropriately", Order = 2, IsActive = true },
                    
                    // Social Emotional Skills (SkillId = 2)
                    new BabyClassSkillItem { Id = 3, SkillId = 2, Name = "Know first name", Description = "Recognition and response to own name", Order = 1, IsActive = true },
                    new BabyClassSkillItem { Id = 4, SkillId = 2, Name = "Follows Instruction", Description = "Ability to follow simple instructions", Order = 2, IsActive = true },
                    new BabyClassSkillItem { Id = 5, SkillId = 2, Name = "Shares well with others", Description = "Cooperative play and sharing behavior", Order = 3, IsActive = true },
                    
                    // Reading & Writing (SkillId = 3)
                    new BabyClassSkillItem { Id = 6, SkillId = 3, Name = "Know how to say letterland characters", Description = "Recognition and pronunciation of letter sounds", Order = 1, IsActive = true },
                    new BabyClassSkillItem { Id = 7, SkillId = 3, Name = "Able to say sounds", Description = "Phonetic awareness and sound production", Order = 2, IsActive = true },
                    
                    // Colour & Shapes (SkillId = 4)
                    new BabyClassSkillItem { Id = 8, SkillId = 4, Name = "Know Primary Colours", Description = "Recognition of basic colors", Order = 1, IsActive = true },
                    new BabyClassSkillItem { Id = 9, SkillId = 4, Name = "Knows Shapes", Description = "Recognition of basic geometric shapes", Order = 2, IsActive = true },
                    
                    // Numbers (SkillId = 5)
                    new BabyClassSkillItem { Id = 10, SkillId = 5, Name = "Able to count", Description = "Basic counting ability", Order = 1, IsActive = true },
                    new BabyClassSkillItem { Id = 11, SkillId = 5, Name = "Orally from 1 - 10", Description = "Verbal counting from 1 to 10", Order = 2, IsActive = true },
                    
                    // Fine-Motor Skills (SkillId = 6)
                    new BabyClassSkillItem { Id = 12, SkillId = 6, Name = "Can hold and use a pencil", Description = "Pencil grip and control", Order = 1, IsActive = true },
                    new BabyClassSkillItem { Id = 13, SkillId = 6, Name = "Can hold and use a Crayon", Description = "Crayon grip and coloring ability", Order = 2, IsActive = true },
                    new BabyClassSkillItem { Id = 14, SkillId = 6, Name = "Able to Trace", Description = "Tracing and copying skills", Order = 3, IsActive = true },
                    
                    // Gross Motor Skills (SkillId = 7)
                    new BabyClassSkillItem { Id = 15, SkillId = 7, Name = "Can jump up and down", Description = "Basic jumping and movement coordination", Order = 1, IsActive = true }
                };

                _context.BabyClassSkillItems.AddRange(skillItems);

                await _context.SaveChangesAsync();
                //_logger.LogInformation("✅ Baby Class Skills seeded successfully");
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "❌ Error seeding Baby Class Skills");
                // Don't throw - this is not critical for app startup
            }
        }

        private async Task SeedStudentExamScoresAsync(Student student)
        {
            try
            {
                // Check if marks already exist for this student for Term 3, End-of-Term, 2025
                var existingMarks = await _context.ExamScores
                    .AnyAsync(es => es.StudentId == student.Id && 
                                   es.AcademicYear == 2025 && 
                                   es.Term == 3);

                if (existingMarks)
                {
                    _logger.LogInformation($"ℹ️ Exam scores already exist for {student.FullName} for Term 3, 2025. Skipping seeding.");
                    return;
                }

                // Get End-of-Term exam type
                var endOfTermExamType = await _context.ExamTypes
                    .FirstOrDefaultAsync(et => et.Name == "End-of-Term");

                if (endOfTermExamType == null)
                {
                    _logger.LogWarning("⚠️ End-of-Term exam type not found. Cannot seed exam scores.");
                    return;
                }

                // Get admin user for RecordedBy
                var adminUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == "admin");

                if (adminUser == null)
                {
                    _logger.LogWarning("⚠️ Admin user not found. Cannot seed exam scores.");
                    return;
                }

                // Get common subjects (Math, English, Science, Social Studies)
                var math = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == "Mathematics" || s.Name.Contains("Math"));
                var english = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == "English");
                var science = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == "Science" || s.Name == "Integrated Science");
                var socialStudies = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == "Social Studies");

                var examScores = new List<ExamScore>();

                // Add Math score
                if (math != null)
                {
                    examScores.Add(new ExamScore
                    {
                        StudentId = student.Id,
                        SubjectId = math.Id,
                        ExamTypeId = endOfTermExamType.Id,
                        GradeId = student.GradeId,
                        Score = 78,
                        IsAbsent = false,
                        AcademicYear = 2025,
                        Term = 3,
                        RecordedAt = DateTime.UtcNow,
                        RecordedBy = adminUser.Id
                    });
                }

                // Add English score
                if (english != null)
                {
                    examScores.Add(new ExamScore
                    {
                        StudentId = student.Id,
                        SubjectId = english.Id,
                        ExamTypeId = endOfTermExamType.Id,
                        GradeId = student.GradeId,
                        Score = 82,
                        IsAbsent = false,
                        AcademicYear = 2025,
                        Term = 3,
                        RecordedAt = DateTime.UtcNow,
                        RecordedBy = adminUser.Id
                    });
                }

                // Add Science score
                if (science != null)
                {
                    examScores.Add(new ExamScore
                    {
                        StudentId = student.Id,
                        SubjectId = science.Id,
                        ExamTypeId = endOfTermExamType.Id,
                        GradeId = student.GradeId,
                        Score = 74,
                        IsAbsent = false,
                        AcademicYear = 2025,
                        Term = 3,
                        RecordedAt = DateTime.UtcNow,
                        RecordedBy = adminUser.Id
                    });
                }

                // Add Social Studies score
                if (socialStudies != null)
                {
                    examScores.Add(new ExamScore
                    {
                        StudentId = student.Id,
                        SubjectId = socialStudies.Id,
                        ExamTypeId = endOfTermExamType.Id,
                        GradeId = student.GradeId,
                        Score = 69,
                        IsAbsent = false,
                        AcademicYear = 2025,
                        Term = 3,
                        RecordedAt = DateTime.UtcNow,
                        RecordedBy = adminUser.Id
                    });
                }

                if (examScores.Any())
                {
                    _context.ExamScores.AddRange(examScores);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Seeded {examScores.Count} exam scores for {student.FullName} (Term 3, End-of-Term, 2025)");
                }
                else
                {
                    _logger.LogWarning($"⚠️ No subjects found to seed exam scores for {student.FullName}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error seeding exam scores for {student.FullName}");
                // Don't throw - this is not critical for app startup
            }
        }
    }
}

    