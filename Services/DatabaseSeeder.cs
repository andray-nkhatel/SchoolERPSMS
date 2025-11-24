using SchoolErpSMS.Data;
using SchoolErpSMS.Entities;
using Microsoft.EntityFrameworkCore;

namespace SchoolErpSMS.Services
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

                // Ensure admin user exists with correct password (root@scherp25)
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                if (adminUser != null)
                {
                    // Update admin password to ensure it's correct
                    adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword("root@scherp25");
                    adminUser.IsActive = true;
                    await _context.SaveChangesAsync();
                    //_logger.LogInformation("✅ Admin user password updated");
                }
                else
                {
                    // Create admin user if it doesn't exist
                    var adminPasswordHash = BCrypt.Net.BCrypt.HashPassword("root@scherp25");
                    var newAdmin = new User
                    {
                        Username = "admin",
                        PasswordHash = adminPasswordHash,
                        FullName = "System Admin",
                        Email = "admin@scherp.sch.edu",
                        Role = UserRole.Admin,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(newAdmin);
                    await _context.SaveChangesAsync();
                    //_logger.LogInformation("✅ Admin user created");
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

                // Seed 5 students with Zambian names, GradeId 2, and exam scores
                _logger.LogInformation("🌱 Starting to seed Zambian students with exam scores...");
                await SeedZambianStudentsWithScoresAsync();
                _logger.LogInformation("✅ Completed seeding Zambian students");

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

        private async Task SeedZambianStudentsWithScoresAsync()
        {
            try
            {
                // First, get or find the grade we need (Form 1, Stream X, or Id 2)
                var grade = await _context.Grades.FirstOrDefaultAsync(g => g.Id == 2);
                if (grade == null)
                {
                    grade = await _context.Grades.FirstOrDefaultAsync(g => 
                        g.Name == "Form 1" && g.Stream == "X");
                }
                
                if (grade == null)
                {
                    _logger.LogWarning("⚠️ Grade 'Form 1 X' not found. Cannot seed students.");
                    return;
                }
                
                // Get AcademicYear for 2025 (should be Id 1 based on seed data)
                var academicYear = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.Id == 1);
                if (academicYear == null)
                {
                    academicYear = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.Name == "2025" || ay.Name.Contains("2025"));
                }
                if (academicYear == null)
                {
                    academicYear = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.IsActive);
                }
                if (academicYear == null)
                {
                    _logger.LogWarning("⚠️ AcademicYear not found. Cannot check existing scores.");
                    return;
                }

                // Get all students with this grade
                var gradeStudents = await _context.Students
                    .Where(s => s.GradeId == grade.Id)
                    .ToListAsync();

                // Check which students have complete scores (9+ subjects) for Term 3, 2025
                var studentsWithCompleteScores = new List<Student>();
                var studentsNeedingScores = new List<Student>();
                
                foreach (var student in gradeStudents)
                {
                    var scoreCount = await _context.ExamScores
                        .CountAsync(es => 
                            es.StudentId == student.Id && 
                            es.AcademicYear == academicYear.Id && 
                            es.Term == 3);
                    
                    if (scoreCount >= 9)
                    {
                        studentsWithCompleteScores.Add(student);
                    }
                    else
                    {
                        studentsNeedingScores.Add(student);
                    }
                }

                _logger.LogInformation($"📊 Found {studentsWithCompleteScores.Count} students with complete scores, {studentsNeedingScores.Count} students needing scores, {gradeStudents.Count} total students in grade.");

                // If we already have 5+ students with complete scores, we're done
                if (studentsWithCompleteScores.Count >= 5)
                {
                    _logger.LogInformation($"ℹ️ Found {studentsWithCompleteScores.Count} students with {grade.Name} {grade.Stream} (Id: {grade.Id}) that already have complete exam scores (9+ subjects) for Term 3, 2025. Skipping seeding.");
                    return;
                }

                _logger.LogInformation($"🌱 Proceeding with seeding for {grade.Name} {grade.Stream} (Id: {grade.Id})...");

                // Zambian first names and last names
                var zambianFirstNames = new[] { "Chanda", "Mwape", "Bwalya", "Mulenga", "Tembo", "Mwansa", "Phiri", "Banda", "Mwanza", "Kunda" };
                var zambianLastNames = new[] { "Mbewe", "Ngoma", "Sichone", "Mwanza", "Kunda", "Banda", "Phiri", "Tembo", "Mulenga", "Mwape" };

                // Get teacher mutale
                var teacherMutale = await _context.Users.FirstOrDefaultAsync(u => u.Username == "mutale");
                if (teacherMutale == null)
                {
                    _logger.LogError("⚠️ Teacher 'mutale' not found. Cannot seed student exam scores.");
                    throw new InvalidOperationException("Teacher 'mutale' not found. Cannot seed student exam scores.");
                }
                _logger.LogInformation($"✅ Found teacher 'mutale' with ID: {teacherMutale.Id}");

                // Get End-of-Term exam type
                var endOfTermExamType = await _context.ExamTypes.FirstOrDefaultAsync(et => et.Name == "End-of-Term");
                if (endOfTermExamType == null)
                {
                    _logger.LogError("⚠️ End-of-Term exam type not found. Cannot seed exam scores.");
                    throw new InvalidOperationException("End-of-Term exam type not found. Cannot seed exam scores.");
                }
                _logger.LogInformation($"✅ Found 'End-of-Term' exam type with ID: {endOfTermExamType.Id}");
                _logger.LogInformation($"✅ Using AcademicYear with ID: {academicYear.Id}, Name: {academicYear.Name}");

                // Get all active subjects and randomly select 9
                var allSubjects = await _context.Subjects.Where(s => s.IsActive).ToListAsync();
                if (allSubjects.Count < 9)
                {
                    _logger.LogError($"⚠️ Not enough subjects found. Found {allSubjects.Count}, need 9.");
                    throw new InvalidOperationException($"Not enough subjects found. Found {allSubjects.Count}, need 9.");
                }
                _logger.LogInformation($"✅ Found {allSubjects.Count} active subjects");

                var random = new Random();
                var selectedSubjects = allSubjects.OrderBy(x => random.Next()).Take(9).ToList();

                // Determine which students to work with
                var studentsToProcess = new List<Student>();
                
                // First, use existing students that need scores
                studentsToProcess.AddRange(studentsNeedingScores);
                
                // If we need more students to reach 5 total, create new ones
                int studentsNeeded = 5 - studentsWithCompleteScores.Count;
                if (studentsNeeded > studentsNeedingScores.Count)
                {
                    int newStudentsToCreate = studentsNeeded - studentsNeedingScores.Count;
                    _logger.LogInformation($"➕ Creating {newStudentsToCreate} new students to reach 5 total...");
                    
                    var usedNames = new HashSet<string>(gradeStudents.Select(s => s.FullName));
                    var newStudents = new List<Student>();

                    for (int i = 0; i < newStudentsToCreate; i++)
                    {
                        string firstName, lastName, fullName;
                        string studentNumber;
                        
                        // Ensure unique names
                        do
                        {
                            firstName = zambianFirstNames[random.Next(zambianFirstNames.Length)];
                            lastName = zambianLastNames[random.Next(zambianLastNames.Length)];
                            fullName = $"{firstName} {lastName}";
                        } while (usedNames.Contains(fullName));
                        
                        usedNames.Add(fullName);

                        // Generate unique student number
                        do
                        {
                            studentNumber = $"24{random.Next(100, 999)}";
                        } while (await _context.Students.AnyAsync(s => s.StudentNumber == studentNumber));

                        var student = new Student
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            StudentNumber = studentNumber,
                            DateOfBirth = new DateTime(2010 + random.Next(-2, 3), random.Next(1, 13), random.Next(1, 29)),
                            Gender = random.Next(2) == 0 ? "Male" : "Female",
                            GradeId = grade.Id,
                            GuardianName = $"{lastName} Family",
                            GuardianPhone = "260975268666",
                            Address = $"Sample Address {i + 1}, Lusaka",
                            EnrollmentDate = DateTime.UtcNow.AddDays(-random.Next(30, 365))
                        };

                        newStudents.Add(student);
                    }

                    _context.Students.AddRange(newStudents);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"✅ Created {newStudents.Count} new students with Zambian names");
                    
                    studentsToProcess.AddRange(newStudents);
                }
                else
                {
                    _logger.LogInformation($"✅ Using {studentsNeedingScores.Count} existing students that need scores");
                }

                // Verify all students to process
                var verifiedStudents = new List<Student>();
                foreach (var student in studentsToProcess)
                {
                    var verified = await _context.Students.FindAsync(student.Id);
                    if (verified != null)
                    {
                        verifiedStudents.Add(verified);
                    }
                }
                
                _logger.LogInformation($"✅ Processing {verifiedStudents.Count} students for exam score creation");
                
                // Log student details
                foreach (var student in verifiedStudents)
                {
                    _logger.LogInformation($"   - {student.FullName} (ID: {student.Id}, Student Number: {student.StudentNumber}, GradeId: {student.GradeId})");
                }

                // Create exam scores for each verified student in the 9 selected subjects
                var examScores = new List<ExamScore>();
                _logger.LogInformation($"📝 Creating exam scores for {verifiedStudents.Count} students in {selectedSubjects.Count} subjects...");
                
                foreach (var student in verifiedStudents)
                {
                    _logger.LogInformation($"   Processing scores for {student.FullName} (ID: {student.Id})...");
                    
                    // Get existing scores for this student for Term 3, 2025
                    var existingScores = await _context.ExamScores
                        .Where(es => 
                            es.StudentId == student.Id && 
                            es.AcademicYear == 2025 && 
                            es.Term == 3)
                        .Select(es => es.SubjectId)
                        .ToListAsync();
                    
                    int scoresAdded = 0;
                    foreach (var subject in selectedSubjects)
                    {
                        // Skip if student already has a score for this subject
                        if (existingScores.Contains(subject.Id))
                        {
                            continue;
                        }
                        
                        // Generate random score between 0 and 100
                        var randomScore = random.Next(0, 101);

                        examScores.Add(new ExamScore
                        {
                            StudentId = student.Id,
                            SubjectId = subject.Id,
                            ExamTypeId = endOfTermExamType.Id,
                            GradeId = grade.Id,
                            Score = randomScore,
                            IsAbsent = false,
                            AcademicYear = academicYear.Id,
                            Term = 3,
                            RecordedAt = DateTime.UtcNow,
                            RecordedBy = teacherMutale.Id
                        });
                        scoresAdded++;
                    }
                    
                    if (scoresAdded > 0)
                    {
                        _logger.LogInformation($"   ✅ Added {scoresAdded} new exam scores for {student.FullName} (already had {existingScores.Count} scores)");
                    }
                    else
                    {
                        _logger.LogInformation($"   ℹ️ {student.FullName} already has all {selectedSubjects.Count} exam scores");
                    }
                }

                _logger.LogInformation($"💾 Saving {examScores.Count} exam scores to database...");
                _context.ExamScores.AddRange(examScores);
                await _context.SaveChangesAsync();

                // Verify exam scores were saved
                var verifiedStudentIds = verifiedStudents.Select(s => s.Id).ToList();
                var savedScoreCount = await _context.ExamScores
                    .CountAsync(es => 
                        verifiedStudentIds.Contains(es.StudentId) && 
                        es.AcademicYear == academicYear.Id && 
                        es.Term == 3);
                
                _logger.LogInformation($"✅ Created and verified {savedScoreCount} exam scores for {verifiedStudents.Count} students in {selectedSubjects.Count} subjects (Term 3, {academicYear.Name})");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error seeding Zambian students with exam scores");
                _logger.LogError($"Error details: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }
                // Re-throw so the caller knows seeding failed
                throw;
            }
        }

        private async Task SeedStudentExamScoresAsync(Student student)
        {
            try
            {
                // Get AcademicYear for 2025 (should be Id 1 based on seed data)
                var academicYear = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.Id == 1);
                if (academicYear == null)
                {
                    academicYear = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.Name == "2025" || ay.Name.Contains("2025"));
                }
                if (academicYear == null)
                {
                    academicYear = await _context.AcademicYears.FirstOrDefaultAsync(ay => ay.IsActive);
                }
                if (academicYear == null)
                {
                    _logger.LogWarning("⚠️ AcademicYear not found. Cannot seed exam scores.");
                    return;
                }

                // Check if marks already exist for this student for Term 3, End-of-Term, 2025
                var existingMarks = await _context.ExamScores
                    .AnyAsync(es => es.StudentId == student.Id && 
                                   es.AcademicYear == academicYear.Id && 
                                   es.Term == 3);

                if (existingMarks)
                {
                    _logger.LogInformation($"ℹ️ Exam scores already exist for {student.FullName} for Term 3, {academicYear.Name}. Skipping seeding.");
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
                        AcademicYear = academicYear.Id,
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
                        AcademicYear = academicYear.Id,
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
                        AcademicYear = academicYear.Id,
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
                        AcademicYear = academicYear.Id,
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

    