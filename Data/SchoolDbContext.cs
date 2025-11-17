using BluebirdCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace BluebirdCore.Data
{
    public class SchoolDbContext : DbContext
    {
        public SchoolDbContext(DbContextOptions<SchoolDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<GradeSubject> GradeSubjects { get; set; }
        public DbSet<StudentOptionalSubject> StudentOptionalSubjects { get; set; }
        public DbSet<TeacherSubjectAssignment> TeacherSubjectAssignments { get; set; }
        public DbSet<ExamType> ExamTypes { get; set; }
        public DbSet<ExamScore> ExamScores { get; set; }
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<ReportCard> ReportCards { get; set; }
        public DbSet<BabyClassSkill> BabyClassSkills { get; set; }
        public DbSet<BabyClassSkillItem> BabyClassSkillItems { get; set; }
        public DbSet<BabyClassSkillAssessment> BabyClassSkillAssessments { get; set; }
        public DbSet<StudentTechnologyTrack> StudentTechnologyTracks { get; set; }
        public DbSet<StudentSubject> StudentSubjects { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Role).HasConversion<int>();
            });


            // Grade configurations
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.Property(e => e.Section).HasConversion<int>();
                entity.HasOne(e => e.HomeroomTeacher)
                      .WithMany(e => e.HomeroomGrades)
                      .HasForeignKey(e => e.HomeroomTeacherId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // GradeSubject configurations
            modelBuilder.Entity<GradeSubject>(entity =>
            {
                entity.HasOne(e => e.Grade)
                      .WithMany(e => e.GradeSubjects)
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Subject)
                      .WithMany(e => e.GradeSubjects)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.GradeId, e.SubjectId }).IsUnique();
            });

            // StudentOptionalSubject configurations
            modelBuilder.Entity<StudentOptionalSubject>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany(e => e.OptionalSubjects)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Subject)
                      .WithMany(e => e.StudentOptionalSubjects)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.StudentId, e.SubjectId }).IsUnique();
            });

            // StudentSubject configurations
            modelBuilder.Entity<StudentSubject>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany(e => e.StudentSubjects)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Subject)
                      .WithMany(e => e.StudentSubjects)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasIndex(e => new { e.StudentId, e.SubjectId }).IsUnique();
            });

            // TeacherSubjectAssignment configurations
            modelBuilder.Entity<TeacherSubjectAssignment>(entity =>
            {
                entity.HasOne(e => e.Teacher)
                      .WithMany(e => e.TeacherAssignments)
                      .HasForeignKey(e => e.TeacherId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Subject)
                      .WithMany(e => e.TeacherAssignments)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Grade)
                      .WithMany()
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ExamScore configurations
            modelBuilder.Entity<ExamScore>(entity =>
            {
                entity.Property(e => e.Score).HasPrecision(5, 2);
                
                entity.HasOne(e => e.Student)
                      .WithMany(e => e.ExamScores)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Subject)
                      .WithMany(e => e.ExamScores)
                      .HasForeignKey(e => e.SubjectId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.ExamType)
                      .WithMany(e => e.ExamScores)
                      .HasForeignKey(e => e.ExamTypeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.Grade)
                      .WithMany()
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.RecordedByTeacher)
                      .WithMany()
                      .HasForeignKey(e => e.RecordedBy)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasIndex(e => new { e.StudentId, e.SubjectId, e.ExamTypeId, e.AcademicYear, e.Term }).IsUnique();
            });

            // ReportCard configurations
            modelBuilder.Entity<ReportCard>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Grade)
                      .WithMany()
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.GeneratedByUser)
                      .WithMany()
                      .HasForeignKey(e => e.GeneratedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // BabyClassSkillAssessment configurations
            modelBuilder.Entity<BabyClassSkillAssessment>(entity =>
            {
                entity.HasOne(e => e.Student)
                      .WithMany()
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.SkillItem)
                      .WithMany(e => e.SkillAssessments)
                      .HasForeignKey(e => e.SkillItemId)
                      .OnDelete(DeleteBehavior.Restrict);
                
                entity.HasOne(e => e.AssessedByNavigation)
                      .WithMany()
                      .HasForeignKey(e => e.AssessedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // BabyClassSkill configurations
            modelBuilder.Entity<BabyClassSkill>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // BabyClassSkillItem configurations
            modelBuilder.Entity<BabyClassSkillItem>(entity =>
            {
                entity.HasOne(e => e.Skill)
                      .WithMany(e => e.SkillItems)
                      .HasForeignKey(e => e.SkillId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed exam types
            modelBuilder.Entity<ExamType>().HasData(
                new ExamType { Id = 1, Name = "Test-One", Description = "First test of the term", Order = 1 },
                new ExamType { Id = 2, Name = "Test-Two", Description = "Second-test examination", Order = 2 },
                new ExamType { Id = 3, Name = "Test-Three", Description = "Third-test examination ( Usually taken by Grade 7)", Order = 3 },
                new ExamType { Id = 4, Name = "End-of-Term", Description = "End of term examination", Order = 4 }
            );

            // Seed academic year
            modelBuilder.Entity<AcademicYear>().HasData(
                new AcademicYear 
                { 
                    Id = 1, 
                    Name = "2025", 
                    StartDate = new DateTime(2025, 1, 1), 
                    EndDate = new DateTime(2025, 12, 1), 
                    IsActive = true 
                }
            );


            // Student configurations
            modelBuilder.Entity<Student>(entity =>
            {
                entity.HasIndex(e => e.StudentNumber).IsUnique();
                entity.HasOne(e => e.Grade)
                      .WithMany(e => e.Students)
                      .HasForeignKey(e => e.GradeId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed admin user (password: admin123)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = "$2a$12$Y5Cr10SW4OuJq6qxj7PXtOhZvb7loVQqIRRwcrH8hsdsoeRCririq",
                    FullName = "System Administrator",
                    Email = "admin@chsschool.com",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt =  new DateTime(2025, 06, 25)
                }
            );

            // Seed subjects by category
            modelBuilder.Entity<Subject>().HasData(
                // PRIMARY & SECONDARY CORE SUBJECTS
                new Subject { Id = 1, Name = "Mathematics", Code = "MATH" },
                new Subject { Id = 2, Name = "English", Code = "ENG" },
                new Subject { Id = 3, Name = "Integrated Science", Code = "SCI" },
                new Subject { Id = 4, Name = "Social Studies", Code = "SS" },
                new Subject { Id = 5, Name = "French", Code = "FR" },
                new Subject { Id = 6, Name = "ICT", Code = "ICT" },
                new Subject { Id = 7, Name = "Physical Education", Code = "PE" },
                new Subject { Id = 8, Name = "Reading", Code = "REA" },
                new Subject { Id = 11, Name = "CTS", Code = "CTS" },
                new Subject { Id = 25, Name = "MDE", Code = "MDE" },
                new Subject { Id = 35, Name = "Cinyanja", Code = "CINY" },
                new Subject { Id = 36, Name = "Science", Code = "SCIE" },
                new Subject { Id = 33, Name = "Computer Studies", Code = "CS" },
                new Subject { Id = 21, Name = "Biology", Code = "BIO" },
                
                // UPPER PRIMARY SPECIAL SUBJECTS
                new Subject { Id = 9, Name = "SP1", Code = "SP1" },
                new Subject { Id = 10, Name = "SP2", Code = "SP2" },
                
                // SECONDARY OPTIONAL SUBJECTS
                new Subject { Id = 12, Name = "Agriculture Science", Code = "AGRI" },
                new Subject { Id = 13, Name = "History", Code = "HIST" },
                new Subject { Id = 14, Name = "Geography", Code = "GEO" },
                new Subject { Id = 15, Name = "Religious Studies", Code = "REL" },
                new Subject { Id = 16, Name = "Business Studies", Code = "BUS" },
                new Subject { Id = 17, Name = "Accounts", Code = "ACC" },
                new Subject { Id = 18, Name = "Economics", Code = "ECO" },
                new Subject { Id = 19, Name = "Physics", Code = "PHY" },
                new Subject { Id = 20, Name = "Chemistry", Code = "CHEM" },
                
                new Subject { Id = 22, Name = "Literature in English", Code = "LIT" },
                new Subject { Id = 23, Name = "Civic Education", Code = "CIV" },
                new Subject { Id = 24, Name = "Music", Code = "MUS" },
                new Subject { Id = 26, Name = "Home Economics", Code = "HE" },
                new Subject { Id = 34, Name = "Computer Science", Code = "COMSCI" },
                new Subject { Id = 37, Name = "Commerce", Code = "COMMER" },
                new Subject { Id = 38, Name = "French", Code = "FRENCH" },
                new Subject { Id = 39, Name = "Art & Design", Code = "ART" },
                new Subject {Id = 40, Name = "Food and Nutrition", Code = "F&N"},
                
                // NEW FORM 1 PATHWAY SUBJECTS
                new Subject { Id = 46, Name = "Design and Technology", Code = "D&T" },
                new Subject { Id = 47, Name = "Fashion and Fabrics", Code = "F&F" },
                new Subject { Id = 48, Name = "Hospitality Management", Code = "HOSP" },
                new Subject { Id = 49, Name = "Travel and Tourism", Code = "T&T" },
                
                // ADDITIONAL PATHWAY SUBJECTS
                new Subject { Id = 50, Name = "Foreign Language", Code = "FL" },
                new Subject { Id = 51, Name = "Zambian Language", Code = "ZL" },
                new Subject { Id = 52, Name = "Principles of Accounts", Code = "POA" },
                new Subject { Id = 53, Name = "Literature in Zambian Language", Code = "LITZL" },
                
                // ELC (EARLY LEARNING CENTER) SUBJECTS
                new Subject { Id = 41, Name = "Language", Code = "LANG" },
                new Subject { Id = 42, Name = "Pre-Math", Code = "PREMATH" },
                new Subject { Id = 43, Name = "Topic", Code = "TOPIC" },
                new Subject { Id = 44, Name = "Creative And Technology Studies (CTS)", Code = "ELCCTS" },
                new Subject { Id = 45, Name = "Creative Activities", Code = "CREAT" },
                
                // ELC DEVELOPMENTAL SKILLS SUBJECTS
                new Subject { Id = 27, Name = "Communication Skills", Code = "COMMSK" },
                new Subject { Id = 28, Name = "Fine-motor Skills", Code = "FM" },
                new Subject { Id = 29, Name = "Gross-motor Skills", Code = "GMS" },
                new Subject { Id = 30, Name = "Social-emotional Skills", Code = "SESK" },
                new Subject { Id = 31, Name = "Colors & Shapes", Code = "COLSS" },
                new Subject { Id = 32, Name = "Numbers", Code = "NUMB" }
            );


            // Seed some grades
            
            modelBuilder.Entity<Grade>().HasData(
               

                 // PRESCHOOL SECTION (Levels 0-8) - Permanent for both systems
                new Grade { Id = 1, Name = "Baby-Class", Stream = "Purple", Level = 1, Section = SchoolSection.EarlyLearningBeginner },
                new Grade { Id = 2, Name = "Baby-Class", Stream = "Green", Level = 1, Section = SchoolSection.EarlyLearningBeginner },
                new Grade { Id = 3, Name = "Baby-Class", Stream = "Orange", Level = 1, Section = SchoolSection.EarlyLearningBeginner },
                new Grade { Id = 4, Name = "Middle-Class", Stream = "Purple", Level = 2, Section = SchoolSection.EarlyLearningIntermediate },
                new Grade { Id = 5, Name = "Middle-Class", Stream = "Green", Level = 2, Section = SchoolSection.EarlyLearningIntermediate},
                new Grade { Id = 6, Name = "Middle-Class", Stream = "Orange", Level = 2, Section = SchoolSection.EarlyLearningIntermediate },
                new Grade { Id = 7, Name = "Reception-Class", Stream = "Purple", Level = 3, Section = SchoolSection.EarlyLearningIntermediate }, 
                new Grade { Id = 8, Name = "Reception-Class", Stream = "Green", Level = 3, Section = SchoolSection.EarlyLearningIntermediate }, 
                new Grade { Id = 9, Name = "Reception-Class", Stream = "Orange", Level = 3, Section = SchoolSection.EarlyLearningIntermediate }, 

                // PRIMARY SECTION
                // Grade 1 - Permanent
                new Grade { Id = 10, Name = "Grade 1", Stream = "Purple", Level = 4, Section = SchoolSection.PrimaryLower,},
                new Grade { Id = 11, Name = "Grade 1", Stream = "Green", Level = 4, Section = SchoolSection.PrimaryLower,},
                new Grade { Id = 12, Name = "Grade 1", Stream = "Orange", Level = 4, Section = SchoolSection.PrimaryLower },
                // Grade 2 - Permanent
                new Grade { Id = 13, Name = "Grade 2", Stream = "Purple", Level = 5, Section = SchoolSection.PrimaryLower },
                new Grade { Id = 14, Name = "Grade 2", Stream = "Green", Level = 5, Section = SchoolSection.PrimaryLower,},
                new Grade { Id = 15, Name = "Grade 2", Stream = "Orange", Level = 5, Section = SchoolSection.PrimaryLower },
                // Grade 3 - Permanent
                new Grade { Id = 16, Name = "Grade 3", Stream = "Purple", Level = 6, Section = SchoolSection.PrimaryLower },
                new Grade { Id = 17, Name = "Grade 3", Stream = "Green", Level = 6, Section = SchoolSection.PrimaryLower},
                new Grade { Id = 18, Name = "Grade 3", Stream = "Orange", Level = 6, Section = SchoolSection.PrimaryLower },
                // Grade 4 - Permanent
                new Grade { Id = 19, Name = "Grade 4", Stream = "Purple", Level = 7, Section = SchoolSection.PrimaryUpper },
                new Grade { Id = 20, Name = "Grade 4", Stream = "Green", Level = 7, Section = SchoolSection.PrimaryUpper},
                new Grade { Id = 21, Name = "Grade 4", Stream = "Orange", Level = 7, Section = SchoolSection.PrimaryUpper },
                // Grade 5 - Permanent
                new Grade { Id = 22, Name = "Grade 5", Stream = "Purple", Level = 8, Section = SchoolSection.PrimaryUpper },
                new Grade { Id = 23, Name = "Grade 5", Stream = "Green", Level = 8, Section = SchoolSection.PrimaryUpper},
                new Grade { Id = 24, Name = "Grade 5", Stream = "Orange", Level = 8, Section = SchoolSection.PrimaryUpper },
                // Grade 6 - Permanent
                new Grade { Id = 25, Name = "Grade 6", Stream = "Purple", Level = 9, Section = SchoolSection.PrimaryUpper },
                new Grade { Id = 26, Name = "Grade 6", Stream = "Green", Level = 9, Section = SchoolSection.PrimaryUpper},
                new Grade { Id = 27, Name = "Grade 6", Stream = "Orange", Level = 9, Section = SchoolSection.PrimaryUpper },
                
                // Grade 7
                new Grade { Id = 28, Name = "Grade 7", Stream = "Purple", Level = 10, Section = SchoolSection.PrimaryUpper },
                new Grade { Id = 29, Name = "Grade 7", Stream = "Green", Level = 10, Section = SchoolSection.PrimaryUpper},
                new Grade { Id = 30, Name = "Grade 7", Stream = "Orange", Level = 10, Section = SchoolSection.PrimaryUpper},

                // SECONDARY SECTION 
                new Grade { Id = 31, Name = "Form 1", Stream = "Grey", Level = 11, Section = SchoolSection.SecondaryJunior},
                new Grade { Id = 32, Name = "Form 1", Stream = "Blue", Level = 11, Section = SchoolSection.SecondaryJunior},
                new Grade { Id = 33, Name = "Grade 9", Stream = "Grey", Level = 11, Section = SchoolSection.SecondaryJunior },
                new Grade { Id = 34, Name = "Grade 9", Stream = "Blue", Level = 11, Section = SchoolSection.SecondaryJunior},
                new Grade { Id = 35, Name = "Grade 10", Stream = "Grey", Level = 12, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 36, Name = "Grade 10", Stream = "Blue", Level = 12, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 37, Name = "Grade 11", Stream = "Grey", Level = 13, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 38, Name = "Grade 11", Stream = "Blue", Level = 13, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 39, Name = "Grade 12", Stream = "Grey", Level = 14, Section = SchoolSection.SecondarySenior},
                new Grade { Id = 40, Name = "Grade 12", Stream = "Blue", Level = 14, Section = SchoolSection.SecondarySenior}

                
                
               
                
          
            );

            // Seed GradeSubjects for lower primary grades (PrimaryLower section)
            // Lower primary grades: Level 4 (Grade 1), Level 5 (Grade 2), Level 6 (Grade 3)
            // Grade IDs: 10, 11, 12 (Grade 1), 13, 14, 15 (Grade 2), 16, 17, 18 (Grade 3)
            // Subject IDs: English=2, Mathematics=1, CTS=11, Integrated Science=3, Social Studies=4, MDE=25, French=5
            modelBuilder.Entity<GradeSubject>().HasData(
                // Lower primary (Grades 1-3, all streams)
                new GradeSubject { Id = 1, GradeId = 10, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 2, GradeId = 10, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 3, GradeId = 10, SubjectId = 11, IsActive = true }, // CTS
                new GradeSubject { Id = 4, GradeId = 10, SubjectId = 3, IsActive = true }, // Integrated Science
                new GradeSubject { Id = 5, GradeId = 10, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 6, GradeId = 10, SubjectId = 25, IsActive = true }, // MDE
                new GradeSubject { Id = 7, GradeId = 10, SubjectId = 5, IsActive = true }, // French
                new GradeSubject { Id = 8, GradeId = 10, SubjectId = 35, IsActive = true }, // Cinyanja
              

                new GradeSubject { Id = 9, GradeId = 11, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 10, GradeId = 11, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 11, GradeId = 11, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 12, GradeId = 11, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 13, GradeId = 11, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 14, GradeId = 11, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 15, GradeId = 11, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 16, GradeId = 11, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 17, GradeId = 12, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 18, GradeId = 12, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 19, GradeId = 12, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 20, GradeId = 12, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 21, GradeId = 12, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 22, GradeId = 12, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 23, GradeId = 12, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 24, GradeId = 12, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 25, GradeId = 13, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 26, GradeId = 13, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 27, GradeId = 13, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 28, GradeId = 13, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 29, GradeId = 13, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 30, GradeId = 13, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 31, GradeId = 13, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 32, GradeId = 13, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 33, GradeId = 14, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 34, GradeId = 14, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 35, GradeId = 14, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 36, GradeId = 14, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 37, GradeId = 14, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 38, GradeId = 14, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 39, GradeId = 14, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 40, GradeId = 14, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 41, GradeId = 15, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 42, GradeId = 15, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 43, GradeId = 15, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 44, GradeId = 15, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 45, GradeId = 15, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 46, GradeId = 15, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 47, GradeId = 15, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 48, GradeId = 15, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 49, GradeId = 16, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 50, GradeId = 16, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 51, GradeId = 16, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 52, GradeId = 16, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 53, GradeId = 16, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 54, GradeId = 16, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 55, GradeId = 16, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 56, GradeId = 16, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 57, GradeId = 17, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 58, GradeId = 17, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 59, GradeId = 17, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 60, GradeId = 17, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 61, GradeId = 17, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 62, GradeId = 17, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 63, GradeId = 17, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 64, GradeId = 17, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 65, GradeId = 18, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 66, GradeId = 18, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 67, GradeId = 18, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 68, GradeId = 18, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 69, GradeId = 18, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 70, GradeId = 18, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 71, GradeId = 18, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 72, GradeId = 18, SubjectId = 35, IsActive = true },
                // Upper primary (Grades 4-7, all streams)
                new GradeSubject { Id = 73, GradeId = 19, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 74, GradeId = 19, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 75, GradeId = 19, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 76, GradeId = 19, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 77, GradeId = 19, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 78, GradeId = 19, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 79, GradeId = 19, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 80, GradeId = 19, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 81, GradeId = 19, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 82, GradeId = 19, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 83, GradeId = 20, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 84, GradeId = 20, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 85, GradeId = 20, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 86, GradeId = 20, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 87, GradeId = 20, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 88, GradeId = 20, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 89, GradeId = 20, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 90, GradeId = 20, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 91, GradeId = 20, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 92, GradeId = 20, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 93, GradeId = 21, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 94, GradeId = 21, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 95, GradeId = 21, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 96, GradeId = 21, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 97, GradeId = 21, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 98, GradeId = 21, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 99, GradeId = 21, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 100, GradeId = 21, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 101, GradeId = 21, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 102, GradeId = 21, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 103, GradeId = 22, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 104, GradeId = 22, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 105, GradeId = 22, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 106, GradeId = 22, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 107, GradeId = 22, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 108, GradeId = 22, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 109, GradeId = 22, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 110, GradeId = 22, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 111, GradeId = 22, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 112, GradeId = 22, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 113, GradeId = 23, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 114, GradeId = 23, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 115, GradeId = 23, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 116, GradeId = 23, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 117, GradeId = 23, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 118, GradeId = 23, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 119, GradeId = 23, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 120, GradeId = 23, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 121, GradeId = 23, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 122, GradeId = 23, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 123, GradeId = 24, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 124, GradeId = 24, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 125, GradeId = 24, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 126, GradeId = 24, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 127, GradeId = 24, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 128, GradeId = 24, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 129, GradeId = 24, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 130, GradeId = 24, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 131, GradeId = 24, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 132, GradeId = 24, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 133, GradeId = 25, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 134, GradeId = 25, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 135, GradeId = 25, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 136, GradeId = 25, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 137, GradeId = 25, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 138, GradeId = 25, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 139, GradeId = 25, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 140, GradeId = 25, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 141, GradeId = 25, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 142, GradeId = 25, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 143, GradeId = 26, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 144, GradeId = 26, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 145, GradeId = 26, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 146, GradeId = 26, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 147, GradeId = 26, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 148, GradeId = 26, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 149, GradeId = 26, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 150, GradeId = 26, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 151, GradeId = 26, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 152, GradeId = 26, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 153, GradeId = 27, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 154, GradeId = 27, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 155, GradeId = 27, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 156, GradeId = 27, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 157, GradeId = 27, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 158, GradeId = 27, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 159, GradeId = 27, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 160, GradeId = 27, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 161, GradeId = 27, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 162, GradeId = 27, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 163, GradeId = 28, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 164, GradeId = 28, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 165, GradeId = 28, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 166, GradeId = 28, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 167, GradeId = 28, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 168, GradeId = 28, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 169, GradeId = 28, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 170, GradeId = 28, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 171, GradeId = 28, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 172, GradeId = 28, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 173, GradeId = 29, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 174, GradeId = 29, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 175, GradeId = 29, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 176, GradeId = 29, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 177, GradeId = 29, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 178, GradeId = 29, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 179, GradeId = 29, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 180, GradeId = 29, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 181, GradeId = 29, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 182, GradeId = 29, SubjectId = 10, IsActive = true },
                new GradeSubject { Id = 183, GradeId = 30, SubjectId = 1, IsActive = true },
                new GradeSubject { Id = 184, GradeId = 30, SubjectId = 2, IsActive = true },
                new GradeSubject { Id = 185, GradeId = 30, SubjectId = 11, IsActive = true },
                new GradeSubject { Id = 186, GradeId = 30, SubjectId = 3, IsActive = true },
                new GradeSubject { Id = 187, GradeId = 30, SubjectId = 4, IsActive = true },
                new GradeSubject { Id = 188, GradeId = 30, SubjectId = 25, IsActive = true },
                new GradeSubject { Id = 189, GradeId = 30, SubjectId = 5, IsActive = true },
                new GradeSubject { Id = 190, GradeId = 30, SubjectId = 35, IsActive = true },
                new GradeSubject { Id = 191, GradeId = 30, SubjectId = 9, IsActive = true },
                new GradeSubject { Id = 192, GradeId = 30, SubjectId = 10, IsActive = true },
                // Secondary grades (Grades 31-40)
                // Core subjects for all secondary grades
                // Form 1 (Grades 31-32) - Core subjects
                new GradeSubject { Id = 193, GradeId = 31, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 194, GradeId = 31, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 195, GradeId = 31, SubjectId = 23, IsActive = true }, // Civic Education
                new GradeSubject { Id = 198, GradeId = 31, SubjectId = 25, IsActive = true }, // MDE
                new GradeSubject { Id = 199, GradeId = 31, SubjectId = 6, IsActive = true }, // ICT

                // Form 1 Blue - Same core subjects
                new GradeSubject { Id = 213, GradeId = 32, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 214, GradeId = 32, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 215, GradeId = 32, SubjectId = 23, IsActive = true }, // Civic Education
                new GradeSubject { Id = 217, GradeId = 32, SubjectId = 25, IsActive = true }, // MDE
                new GradeSubject { Id = 218, GradeId = 32, SubjectId = 6, IsActive = true }, // ICT

                // Grade 9 (Grades 33-34)
                new GradeSubject { Id = 231, GradeId = 33, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 232, GradeId = 33, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 233, GradeId = 33, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 234, GradeId = 33, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 235, GradeId = 33, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 236, GradeId = 33, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 9 Grey
                new GradeSubject { Id = 237, GradeId = 33, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 238, GradeId = 33, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 239, GradeId = 33, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 240, GradeId = 33, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 241, GradeId = 33, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 242, GradeId = 33, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 243, GradeId = 33, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 244, GradeId = 33, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 245, GradeId = 33, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 246, GradeId = 33, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 247, GradeId = 33, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 248, GradeId = 33, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 9 Blue
                new GradeSubject { Id = 249, GradeId = 34, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 250, GradeId = 34, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 251, GradeId = 34, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 252, GradeId = 34, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 253, GradeId = 34, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 254, GradeId = 34, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 9 Blue
                new GradeSubject { Id = 255, GradeId = 34, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 256, GradeId = 34, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 257, GradeId = 34, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 258, GradeId = 34, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 259, GradeId = 34, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 260, GradeId = 34, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 261, GradeId = 34, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 262, GradeId = 34, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 263, GradeId = 34, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 264, GradeId = 34, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 265, GradeId = 34, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 266, GradeId = 34, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 10 (Grades 35-36)
                new GradeSubject { Id = 267, GradeId = 35, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 268, GradeId = 35, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 269, GradeId = 35, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 270, GradeId = 35, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 271, GradeId = 35, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 272, GradeId = 35, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 10 Grey
                new GradeSubject { Id = 273, GradeId = 35, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 274, GradeId = 35, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 275, GradeId = 35, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 276, GradeId = 35, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 277, GradeId = 35, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 278, GradeId = 35, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 279, GradeId = 35, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 280, GradeId = 35, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 281, GradeId = 35, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 282, GradeId = 35, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 283, GradeId = 35, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 284, GradeId = 35, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 10 Blue
                new GradeSubject { Id = 285, GradeId = 36, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 286, GradeId = 36, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 287, GradeId = 36, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 288, GradeId = 36, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 289, GradeId = 36, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 290, GradeId = 36, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 10 Blue
                new GradeSubject { Id = 291, GradeId = 36, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 292, GradeId = 36, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 293, GradeId = 36, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 294, GradeId = 36, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 295, GradeId = 36, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 296, GradeId = 36, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 297, GradeId = 36, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 298, GradeId = 36, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 299, GradeId = 36, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 300, GradeId = 36, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 301, GradeId = 36, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 302, GradeId = 36, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 11 (Grades 37-38)
                new GradeSubject { Id = 303, GradeId = 37, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 304, GradeId = 37, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 305, GradeId = 37, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 306, GradeId = 37, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 307, GradeId = 37, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 308, GradeId = 37, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 11 Grey
                new GradeSubject { Id = 309, GradeId = 37, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 310, GradeId = 37, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 311, GradeId = 37, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 312, GradeId = 37, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 313, GradeId = 37, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 314, GradeId = 37, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 315, GradeId = 37, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 316, GradeId = 37, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 317, GradeId = 37, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 318, GradeId = 37, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 319, GradeId = 37, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 320, GradeId = 37, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 11 Blue
                new GradeSubject { Id = 321, GradeId = 38, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 322, GradeId = 38, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 323, GradeId = 38, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 324, GradeId = 38, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 325, GradeId = 38, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 326, GradeId = 38, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 11 Blue
                new GradeSubject { Id = 327, GradeId = 38, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 328, GradeId = 38, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 329, GradeId = 38, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 330, GradeId = 38, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 331, GradeId = 38, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 332, GradeId = 38, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 333, GradeId = 38, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 334, GradeId = 38, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 335, GradeId = 38, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 336, GradeId = 38, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 337, GradeId = 38, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 338, GradeId = 38, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 12 (Grades 39-40)
                new GradeSubject { Id = 339, GradeId = 39, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 340, GradeId = 39, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 341, GradeId = 39, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 342, GradeId = 39, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 343, GradeId = 39, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 344, GradeId = 39, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 12 Grey
                new GradeSubject { Id = 345, GradeId = 39, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 346, GradeId = 39, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 347, GradeId = 39, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 348, GradeId = 39, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 349, GradeId = 39, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 350, GradeId = 39, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 351, GradeId = 39, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 352, GradeId = 39, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 353, GradeId = 39, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 354, GradeId = 39, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 355, GradeId = 39, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 356, GradeId = 39, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                // Grade 12 Blue
                new GradeSubject { Id = 357, GradeId = 40, SubjectId = 2, IsActive = true }, // English
                new GradeSubject { Id = 358, GradeId = 40, SubjectId = 1, IsActive = true }, // Mathematics
                new GradeSubject { Id = 359, GradeId = 40, SubjectId = 4, IsActive = true }, // Social Studies
                new GradeSubject { Id = 360, GradeId = 40, SubjectId = 36, IsActive = true }, // Science
                new GradeSubject { Id = 361, GradeId = 40, SubjectId = 33, IsActive = true }, // Computer Studies
                new GradeSubject { Id = 362, GradeId = 40, SubjectId = 25, IsActive = true }, // MDE
                // Optional subjects for Grade 12 Blue
                new GradeSubject { Id = 363, GradeId = 40, SubjectId = 17, IsActive = true, IsOptional = true }, // Accounts
                new GradeSubject { Id = 364, GradeId = 40, SubjectId = 22, IsActive = true, IsOptional = true }, // Literature in English
                new GradeSubject { Id = 365, GradeId = 40, SubjectId = 12, IsActive = true, IsOptional = true }, // Agriculture Science
                new GradeSubject { Id = 366, GradeId = 40, SubjectId = 15, IsActive = true, IsOptional = true }, // Religious Education
                new GradeSubject { Id = 367, GradeId = 40, SubjectId = 37, IsActive = true, IsOptional = true }, // Commerce
                new GradeSubject { Id = 368, GradeId = 40, SubjectId = 26, IsActive = true, IsOptional = true }, // Home Economics
                new GradeSubject { Id = 369, GradeId = 40, SubjectId = 24, IsActive = true, IsOptional = true }, // Music
                new GradeSubject { Id = 370, GradeId = 40, SubjectId = 16, IsActive = true, IsOptional = true }, // Business Studies
                new GradeSubject { Id = 371, GradeId = 40, SubjectId = 13, IsActive = true, IsOptional = true }, // History
                new GradeSubject { Id = 372, GradeId = 40, SubjectId = 14, IsActive = true, IsOptional = true }, // Geography
                new GradeSubject { Id = 373, GradeId = 40, SubjectId = 5, IsActive = true, IsOptional = true }, // French
                new GradeSubject { Id = 374, GradeId = 40, SubjectId = 39, IsActive = true, IsOptional = true }, // Art & Design

                
                // ELC GradeSubject relationships
                // Baby Class (Grades 1-3)
                new GradeSubject { Id = 375, GradeId = 1, SubjectId = 40, IsActive = true }, // LANGUAGE
                new GradeSubject { Id = 376, GradeId = 1, SubjectId = 41, IsActive = true }, // PREMATHS
                new GradeSubject { Id = 377, GradeId = 1, SubjectId = 42, IsActive = true }, // TOPIC
                new GradeSubject { Id = 378, GradeId = 1, SubjectId = 44, IsActive = true }, // CREATIVE ACTIVITIES
                new GradeSubject { Id = 379, GradeId = 2, SubjectId = 40, IsActive = true }, // LANGUAGE
                new GradeSubject { Id = 380, GradeId = 2, SubjectId = 41, IsActive = true }, // PREMATHS
                new GradeSubject { Id = 381, GradeId = 2, SubjectId = 42, IsActive = true }, // TOPIC
                new GradeSubject { Id = 382, GradeId = 2, SubjectId = 44, IsActive = true }, // CREATIVE ACTIVITIES
                new GradeSubject { Id = 383, GradeId = 3, SubjectId = 40, IsActive = true }, // LANGUAGE
                new GradeSubject { Id = 384, GradeId = 3, SubjectId = 41, IsActive = true }, // PREMATHS
                new GradeSubject { Id = 385, GradeId = 3, SubjectId = 42, IsActive = true }, // TOPIC
                new GradeSubject { Id = 386, GradeId = 3, SubjectId = 44, IsActive = true }, // CREATIVE ACTIVITIES
                
                // Middle Class (Grades 4-6)
                new GradeSubject { Id = 387, GradeId = 4, SubjectId = 40, IsActive = true }, // LANGUAGE
                new GradeSubject { Id = 388, GradeId = 4, SubjectId = 41, IsActive = true }, // PREMATHS
                new GradeSubject { Id = 389, GradeId = 4, SubjectId = 42, IsActive = true }, // TOPIC
                new GradeSubject { Id = 390, GradeId = 4, SubjectId = 43, IsActive = true }, // CREATIVE AND TECHNOLOGY STUDIES (CTS)
                new GradeSubject { Id = 391, GradeId = 5, SubjectId = 40, IsActive = true }, // LANGUAGE
                new GradeSubject { Id = 392, GradeId = 5, SubjectId = 41, IsActive = true }, // PREMATHS
                new GradeSubject { Id = 393, GradeId = 5, SubjectId = 42, IsActive = true }, // TOPIC
                new GradeSubject { Id = 394, GradeId = 5, SubjectId = 43, IsActive = true }, // CREATIVE AND TECHNOLOGY STUDIES (CTS)
                new GradeSubject { Id = 395, GradeId = 6, SubjectId = 40, IsActive = true }, // LANGUAGE
                new GradeSubject { Id = 396, GradeId = 6, SubjectId = 41, IsActive = true }, // PREMATHS
                new GradeSubject { Id = 397, GradeId = 6, SubjectId = 42, IsActive = true }, // TOPIC
                new GradeSubject { Id = 398, GradeId = 6, SubjectId = 43, IsActive = true }, // CREATIVE AND TECHNOLOGY STUDIES (CTS)
                
                // Reception Class (Grades 7-9)
                new GradeSubject { Id = 399, GradeId = 7, SubjectId = 40, IsActive = true }, // LANGUAGE
                new GradeSubject { Id = 400, GradeId = 7, SubjectId = 41, IsActive = true }, // PREMATHS
                new GradeSubject { Id = 401, GradeId = 7, SubjectId = 42, IsActive = true }, // TOPIC
                new GradeSubject { Id = 402, GradeId = 7, SubjectId = 43, IsActive = true }, // CREATIVE AND TECHNOLOGY STUDIES (CTS)
                new GradeSubject { Id = 403, GradeId = 7, SubjectId = 5, IsActive = true }, // FRENCH
                new GradeSubject { Id = 404, GradeId = 8, SubjectId = 40, IsActive = true }, // LANGUAGE
                new GradeSubject { Id = 405, GradeId = 8, SubjectId = 41, IsActive = true }, // PREMATHS
                new GradeSubject { Id = 406, GradeId = 8, SubjectId = 42, IsActive = true }, // TOPIC
                new GradeSubject { Id = 407, GradeId = 8, SubjectId = 43, IsActive = true }, // CREATIVE AND TECHNOLOGY STUDIES (CTS)
                new GradeSubject { Id = 408, GradeId = 8, SubjectId = 5, IsActive = true }, // FRENCH
                new GradeSubject { Id = 409, GradeId = 9, SubjectId = 40, IsActive = true }, // LANGUAGE
                new GradeSubject { Id = 410, GradeId = 9, SubjectId = 41, IsActive = true }, // PREMATHS
                new GradeSubject { Id = 411, GradeId = 9, SubjectId = 42, IsActive = true }, // TOPIC
                new GradeSubject { Id = 412, GradeId = 9, SubjectId = 43, IsActive = true }, // CREATIVE AND TECHNOLOGY STUDIES (CTS)
                new GradeSubject { Id = 413, GradeId = 9, SubjectId = 5, IsActive = true } // FRENCH
            );

            // Add Reading (SubjectId = 8) as a core subject for every GradeId from 10 to 40
            modelBuilder.Entity<GradeSubject>().HasData(
                new GradeSubject { Id = 4000, GradeId = 10, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4001, GradeId = 11, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4002, GradeId = 12, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4003, GradeId = 13, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4004, GradeId = 14, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4005, GradeId = 15, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4006, GradeId = 16, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4007, GradeId = 17, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4008, GradeId = 18, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4009, GradeId = 19, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4010, GradeId = 20, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4011, GradeId = 21, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4012, GradeId = 22, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4013, GradeId = 23, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4014, GradeId = 24, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4015, GradeId = 25, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4016, GradeId = 26, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4017, GradeId = 27, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4018, GradeId = 28, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4019, GradeId = 29, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4020, GradeId = 30, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4021, GradeId = 31, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4022, GradeId = 32, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4023, GradeId = 33, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4024, GradeId = 34, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4025, GradeId = 35, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4026, GradeId = 36, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4027, GradeId = 37, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4028, GradeId = 38, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4029, GradeId = 39, SubjectId = 8, IsActive = true }, // Reading
                new GradeSubject { Id = 4030, GradeId = 40, SubjectId = 8, IsActive = true } // Reading
            );

            // Seed Baby Class Skills
            modelBuilder.Entity<BabyClassSkill>().HasData(
                new BabyClassSkill { Id = 1, Name = "Communication Skills", Description = "Language and communication development", Order = 1, IsActive = true },
                new BabyClassSkill { Id = 2, Name = "Fine Motor Skills", Description = "Hand-eye coordination and dexterity", Order = 2, IsActive = true },
                new BabyClassSkill { Id = 3, Name = "Gross Motor Skills", Description = "Large muscle movement and coordination", Order = 3, IsActive = true },
                new BabyClassSkill { Id = 4, Name = "Social-Emotional Skills", Description = "Social interaction and emotional development", Order = 4, IsActive = true },
                new BabyClassSkill { Id = 5, Name = "Cognitive Skills", Description = "Thinking and problem-solving abilities", Order = 5, IsActive = true }
            );

            // Seed Baby Class Skill Items
            modelBuilder.Entity<BabyClassSkillItem>().HasData(
                // Communication Skills
                new BabyClassSkillItem { Id = 1, Name = "Speaks clearly", Description = "Can articulate words clearly", SkillId = 1, Order = 1, IsActive = true },
                new BabyClassSkillItem { Id = 2, Name = "Listens attentively", Description = "Pays attention when spoken to", SkillId = 1, Order = 2, IsActive = true },
                new BabyClassSkillItem { Id = 3, Name = "Follows instructions", Description = "Can follow simple 2-3 step instructions", SkillId = 1, Order = 3, IsActive = true },
                
                // Fine Motor Skills
                new BabyClassSkillItem { Id = 4, Name = "Holds pencil correctly", Description = "Proper pencil grip", SkillId = 2, Order = 1, IsActive = true },
                new BabyClassSkillItem { Id = 5, Name = "Cuts with scissors", Description = "Can use safety scissors", SkillId = 2, Order = 2, IsActive = true },
                new BabyClassSkillItem { Id = 6, Name = "Draws basic shapes", Description = "Can draw circles, squares, triangles", SkillId = 2, Order = 3, IsActive = true },
                
                // Gross Motor Skills
                new BabyClassSkillItem { Id = 7, Name = "Runs confidently", Description = "Can run without falling", SkillId = 3, Order = 1, IsActive = true },
                new BabyClassSkillItem { Id = 8, Name = "Jumps with both feet", Description = "Can jump forward and backward", SkillId = 3, Order = 2, IsActive = true },
                new BabyClassSkillItem { Id = 9, Name = "Balances on one foot", Description = "Can balance for 3-5 seconds", SkillId = 3, Order = 3, IsActive = true },
                
                // Social-Emotional Skills
                new BabyClassSkillItem { Id = 10, Name = "Shares with others", Description = "Willingly shares toys and materials", SkillId = 4, Order = 1, IsActive = true },
                new BabyClassSkillItem { Id = 11, Name = "Takes turns", Description = "Waits for turn in group activities", SkillId = 4, Order = 2, IsActive = true },
                new BabyClassSkillItem { Id = 12, Name = "Shows empathy", Description = "Shows concern for others' feelings", SkillId = 4, Order = 3, IsActive = true },
                
                // Cognitive Skills
                new BabyClassSkillItem { Id = 13, Name = "Recognizes colors", Description = "Can identify primary colors", SkillId = 5, Order = 1, IsActive = true },
                new BabyClassSkillItem { Id = 14, Name = "Counts to 10", Description = "Can count objects up to 10", SkillId = 5, Order = 2, IsActive = true },
                new BabyClassSkillItem { Id = 15, Name = "Sorts objects", Description = "Can group objects by attributes", SkillId = 5, Order = 3, IsActive = true }
            );
        }
    }
}